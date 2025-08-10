using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Discord;
using Discord.WebSocket;
using MaxMind.GeoIP2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleDiscordRelay;

public class PlayerIpInfo
{
    public string IpAddress { get; set; } = string.Empty;
}

public partial class SimpleDiscordRelay : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "Simple Discord Relay";
    public override string ModuleAuthor => "Tsukasa";
    public override string ModuleVersion => "1.0.0";

    public PluginConfig Config { get; set; } = new();

    private DiscordSocketClient? _client;
    private DatabaseReader? _geoIpReader;
    
    private readonly HashSet<ulong> _reconnectingPlayers = new();
    private readonly Dictionary<int, DateTime> _lastChatTimes = new();
    private readonly Dictionary<ulong, PlayerIpInfo> _playerIpCache = new();
    private readonly Dictionary<ulong, CounterStrikeSharp.API.Modules.Timers.Timer> _pendingDisconnects = new();

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        if (string.IsNullOrEmpty(Config.BotToken) || Config.BotToken == "YOUR_BOT_TOKEN_HERE")
        {
            Console.WriteLine("[Discord Relay] Error: Bot Token is not configured. Please set it in the plugin config.");
            return;
        }

        string geoIpDbPath = Path.Combine(ModuleDirectory, "GeoLite2-Country.mmdb");
        if (!File.Exists(geoIpDbPath))
        {
            Console.WriteLine("[Discord Relay] GeoLite2-Country.mmdb not found. Country flags will be disabled.");
        }
        else
        {
            _geoIpReader = new DatabaseReader(geoIpDbPath);
        }

        if (Config.MapChangeNotificationsEnabled)
        {
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        }
        
        Task.Run(async () => await InitializeDiscordBot());
    }

    public override void Unload(bool hotReload)
    {
        _geoIpReader?.Dispose();
        if (_client != null)
        {
            Task.Run(async () => await _client.StopAsync());
        }
    }

    private async Task InitializeDiscordBot()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        });

        _client.Log += LogAsync;
        _client.Ready += OnReadyAsync;
        _client.MessageReceived += OnMessageReceivedAsync;
        
        await _client.LoginAsync(TokenType.Bot, Config.BotToken);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine($"[Discord] {log.ToString()}");
        return Task.CompletedTask;
    }

    private Task OnReadyAsync()
    {
        Console.WriteLine("[Discord] Bot is connected!");
        if (Config.BotStatusEnabled)
        {
            AddTimer(30.0f, UpdateBotStatus, TimerFlags.REPEAT);
        }
        return Task.CompletedTask;
    }

    private Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (_client == null || message.Author.Id == _client.CurrentUser.Id || message.Author.IsBot)
        {
            return Task.CompletedTask;
        }

        if (!Config.ChatRelayEnabled || message.Channel.Id != Config.ChatRelayChannelId)
        {
            return Task.CompletedTask;
        }
        
        var author = message.Author as SocketGuildUser;
        string authorName = author?.DisplayName ?? message.Author.Username;

        Server.NextFrame(() =>
        {
            Server.PrintToChatAll($" {ChatColors.Green}[Discord] {ChatColors.Default}{authorName} ({message.Author.Username}): {ChatColors.White}{message.Content}");
        });
        return Task.CompletedTask;
    }

    [GameEventHandler]
    public HookResult OnPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if (!Config.ChatRelayEnabled)
            return HookResult.Continue;

        var player = Utilities.GetPlayerFromUserid(@event.Userid);
        if (player == null || !player.IsValid || player.AuthorizedSteamID == null || player.UserId == null)
            return HookResult.Continue;

        int userId = player.UserId.Value;
        if (_lastChatTimes.TryGetValue(userId, out var lastChat) && (DateTime.UtcNow - lastChat).TotalMilliseconds < 100)
        {
            return HookResult.Continue;
        }
        _lastChatTimes[userId] = DateTime.UtcNow;

        var message = @event.Text;
        if (string.IsNullOrWhiteSpace(message))
            return HookResult.Continue;

        string playerName = player.PlayerName;
        string profileUrl = player.AuthorizedSteamID.ToCommunityUrl().ToString();
        string countryEmoji = GetPlayerCountryEmoji(player.IpAddress);
        string prefix = @event.Teamonly ? "[TEAM] " : "";
        string sanitizedMessage = message.Replace("@", "@\u200B");

        string discordMessage = $"{countryEmoji} **{prefix}[{playerName}](<{profileUrl}>)**: {sanitizedMessage}";
        Task.Run(async () => await SendDiscordMessageAsync(Config.ChatRelayChannelId, discordMessage));

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid == null)
            return HookResult.Continue;

        var player = @event.Userid;
        if (player.IsBot || !player.IsValid || player.AuthorizedSteamID == null)
            return HookResult.Continue;

        ulong steamId64 = player.AuthorizedSteamID.SteamId64;

        if (player.IpAddress != null)
        {
            _playerIpCache[steamId64] = new PlayerIpInfo { IpAddress = player.IpAddress };
        }
        
        // If a disconnect timer exists for this player, kill it.
        if (_pendingDisconnects.TryGetValue(steamId64, out var disconnectTimer))
        {
            disconnectTimer.Kill();
            _pendingDisconnects.Remove(steamId64);
        }
        
        if (_reconnectingPlayers.Remove(steamId64))
        {
            return HookResult.Continue;
        }

        if (!Config.JoinNotificationsEnabled)
            return HookResult.Continue;

        string playerName = player.PlayerName;
        string profileUrl = player.AuthorizedSteamID.ToCommunityUrl().ToString();
        string countryEmoji = GetPlayerCountryEmoji(player.IpAddress);

        Task.Run(async () => await SendDiscordMessageAsync(Config.ServerChannelId, $":arrow_right: {countryEmoji} **[{playerName}](<{profileUrl}>)** has connected."));
        
        if (Config.BotStatusEnabled)
        {
            AddTimer(1.0f, UpdateBotStatus);
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid == null || @event.Networkid == "BOT")
            return HookResult.Continue;
            
        var steamId = new SteamID(@event.Networkid);
        ulong steamId64 = steamId.SteamId64;

        if (!Config.LeaveNotificationsEnabled)
        {
            _playerIpCache.Remove(steamId64);
            return HookResult.Continue;
        }

        string playerName = @event.Name;
        string profileUrl = steamId.ToCommunityUrl().ToString();
        
        string countryEmoji = ":flag_white:";
        if (_playerIpCache.TryGetValue(steamId64, out var ipInfo))
        {
            countryEmoji = GetPlayerCountryEmoji(ipInfo.IpAddress);
            _playerIpCache.Remove(steamId64);
        }

        var disconnectTimer = AddTimer(5.0f, () =>
        {
            Task.Run(async () => await SendDiscordMessageAsync(Config.ServerChannelId, $":arrow_left: {countryEmoji} **[{playerName}](<{profileUrl}>)** has disconnected."));
            _pendingDisconnects.Remove(steamId64);
        });
        _pendingDisconnects[steamId64] = disconnectTimer;
        
        if (Config.BotStatusEnabled)
        {
            AddTimer(1.0f, UpdateBotStatus);
        }
        return HookResult.Continue;
    }

    private void OnMapStart(string mapName)
    {
        if (mapName.Equals("ar_baggage", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Task.Run(async () => await SendDiscordMessageAsync(Config.ServerChannelId, $":map: Map changed to **{mapName}**."));
        if (Config.BotStatusEnabled)
        {
            UpdateBotStatus();
        }
    }

    private void OnMapEnd()
    {
        _reconnectingPlayers.Clear();
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.AuthorizedSteamID != null))
        {
            _reconnectingPlayers.Add(player.AuthorizedSteamID.SteamId64);
        }
    }

    private async Task SendDiscordMessageAsync(ulong channelId, string message)
    {
        if (_client == null || _client.ConnectionState != ConnectionState.Connected)
        {
            Console.WriteLine("[Discord Relay] Error: Discord client is not ready or initialized.");
            return;
        }
        if (channelId == 0)
        {
            Console.WriteLine("[Discord Relay] Error: Cannot send message because the target Channel ID is 0. Please check your config.");
            return;
        }

        try
        {
            var channel = await _client.GetChannelAsync(channelId) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine($"[Discord Relay] Error: Channel with ID {channelId} not found. The bot may not have access to it.");
                return;
            }

            await channel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Discord Relay] FATAL: An exception occurred while sending a message to channel {channelId}.");
            Console.WriteLine($"[Discord Relay] Details: {ex.Message}");
        }
    }

    private void UpdateBotStatus()
    {
        if (_client == null || !Config.BotStatusEnabled || _client.ConnectionState != ConnectionState.Connected)
        {
            return;
        }

        var playerCount = Utilities.GetPlayers().Count(p => p.IsValid && !p.IsBot);
        var maxPlayers = Server.MaxPlayers;
        var mapName = Server.MapName;

        string status = Config.BotStatusMessage
            .Replace("{PlayerCount}", playerCount.ToString())
            .Replace("{MaxPlayers}", maxPlayers.ToString())
            .Replace("{MapName}", mapName);

        Task.Run(async () => await _client.SetGameAsync(status));
    }

    private string GetPlayerCountryEmoji(string? ipAddress)
    {
        if (_geoIpReader == null || string.IsNullOrWhiteSpace(ipAddress))
        {
            return ":flag_white:";
        }

        var ipOnly = ipAddress.Split(':')[0];
        if (ipOnly == "127.0.0.1" || ipOnly.Contains("localhost"))
        {
            return ":flag_white:";
        }

        try
        {
            var country = _geoIpReader.Country(ipOnly);
            if (country?.Country?.IsoCode != null)
            {
                return $":flag_{country.Country.IsoCode.ToLower()}:";
            }
        }
        catch (Exception)
        {
            // Ignore
        }

        return ":flag_white:";
    }
}