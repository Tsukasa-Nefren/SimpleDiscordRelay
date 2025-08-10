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

    // --- Improvement #2: Added Main Configuration Values ---
    [JsonPropertyName("DisconnectDelaySeconds")]
    public float DisconnectDelaySeconds { get; set; } = 5.0f;

    [JsonPropertyName("ChatSpamCooldownMs")]
    public int ChatSpamCooldownMs { get; set; } = 100;

    // --- Improvement #1: Added Embed Settings ---
    [JsonPropertyName("UseEmbeds")]
    public bool UseEmbeds { get; set; } = true;

    [JsonPropertyName("EmbedColor_Connect")]
    public string EmbedColorConnect { get; set; } = "00FF00"; // Green

    [JsonPropertyName("EmbedColor_Disconnect")]
    public string EmbedColorDisconnect { get; set; } = "FF0000"; // Red

    [JsonPropertyName("EmbedColor_MapChange")]
    public string EmbedColorMapChange { get; set; } = "0000FF"; // Blue

    [JsonPropertyName("EmbedColor_Chat")]
    public string EmbedColorChat { get; set; } = "FFFFFF"; // White

    // --- Improvement #3: Message Format Customization ---
    // Available variables: {player_name}, {profile_url}, {country_emoji}, {message}, {team_prefix}
    [JsonPropertyName("MessageFormat_GameToDiscord")]
    public string GameToDiscordFormat { get; set; } = "{country_emoji} **{team_prefix}[{player_name}](<{profile_url}>)**: {message}";

    // Available variables: {author_name}, {author_username}, {message}
    [JsonPropertyName("MessageFormat_DiscordToGame")]
    public string DiscordToGameFormat { get; set; } = " {Green}[Discord] {Default}{author_name} ({author_username}): {White}{message}";

    // Available variables: {player_name}, {profile_url}, {country_emoji}
    [JsonPropertyName("MessageFormat_PlayerConnect")]
    public string PlayerConnectFormat { get; set; } = ":arrow_right: {country_emoji} **[{player_name}](<{profile_url}>)** has connected.";

    // Available variables: {player_name}, {profile_url}, {country_emoji}
    [JsonPropertyName("MessageFormat_PlayerDisconnect")]
    public string PlayerDisconnectFormat { get; set; } = ":arrow_left: {country_emoji} **[{player_name}](<{profile_url}>)** has disconnected.";

    // Available variables: {map_name}
    [JsonPropertyName("MessageFormat_MapChange")]
    public string MapChangeFormat { get; set; } = ":map: Map changed to **{map_name}**.";
}