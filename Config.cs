using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace SimpleDiscordRelay;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("BotToken")]
    public string BotToken { get; set; } = "YOUR_BOT_TOKEN_HERE";

    [JsonPropertyName("ServerChannelId")]
    public ulong ServerChannelId { get; set; } = 0;

    [JsonPropertyName("ChatRelayEnabled")]
    public bool ChatRelayEnabled { get; set; } = true;

    [JsonPropertyName("ChatRelayChannelId")]
    public ulong ChatRelayChannelId { get; set; } = 0;

    [JsonPropertyName("JoinNotificationsEnabled")]
    public bool JoinNotificationsEnabled { get; set; } = true;

    [JsonPropertyName("LeaveNotificationsEnabled")]
    public bool LeaveNotificationsEnabled { get; set; } = true;

    [JsonPropertyName("MapChangeNotificationsEnabled")]
    public bool MapChangeNotificationsEnabled { get; set; } = true;

    [JsonPropertyName("BotStatusEnabled")]
    public bool BotStatusEnabled { get; set; } = true;

    [JsonPropertyName("BotStatusMessage")]
    public string BotStatusMessage { get; set; } = "{MapName} | {PlayerCount}/{MaxPlayers}";
}