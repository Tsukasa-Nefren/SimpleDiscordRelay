# SimpleDiscordRelay for Counter-Strike 2

A powerful and highly customizable Discord relay plugin for Counter-Strike 2, built on the CounterStrikeSharp framework. This plugin seamlessly integrates your game server with your Discord community.

## Features

- **Two-Way Chat Relay:** Relays in-game chat to Discord and vice-versa.
- **Rich Embed Notifications:** Utilizes Discord embeds for clean, professional-looking notifications with color-coding for different events (connections, disconnections, map changes).
- **Full Customization:** Almost every aspect is configurable, from enabling/disabling features to customizing message formats and embed colors.
- **Player Event Notifications:** Announce when players connect, disconnect, or when the map changes.
- **Accurate Reconnect Handling:** Intelligently handles player reconnects to prevent notification spam.
- **GeoIP Country Flags:** Displays a player's country flag next to their name.
- **Steam Profile Links:** Player names in notifications are automatically linked to their Steam profiles.
- **Configurable Bot Status:** Sets the bot's status message to display the current map and player count.

## Installation

1.  **Download the latest release:** Go to the **Releases** page of this repository and download the latest version.
2.  **Extract:** Unzip the downloaded file. The required `GeoLite2-Country.mmdb` file is included.
3.  **Upload Files:**
    - Create a new folder named `SimpleDiscordRelay` inside your server's `addons/counterstrikesharp/plugins` directory.
    - Upload all the extracted files into this new folder.
4.  **First Run & Configuration:**
    - Start your server once to generate the `config.json` file in `addons/counterstrikesharp/plugins/SimpleDiscordRelay/`.
    - Open the `config.json` file and configure it according to the options below.
5.  **Restart:** Restart your server or load the plugin manually for the changes to take effect.

## Configuration (`config.json`)

Below is a detailed explanation of all available configuration options.

### Core Settings
| Key | Type | Description |
| --- | --- | --- |
| `BotToken` | string | Your Discord Bot's secret token. **Required.** |
| `ServerChannelId` | ulong | The ID of the Discord channel for server notifications (connect/disconnect/map change). |
| `ChatRelayEnabled` | bool | Enables or disables the chat relay feature. |
| `ChatRelayChannelId` | ulong | The ID of the Discord channel for two-way chat relay. |
| `JoinNotificationsEnabled` | bool | If `true`, sends a message when a player joins. |
| `LeaveNotificationsEnabled` | bool | If `true`, sends a message when a player leaves. |
| `MapChangeNotificationsEnabled` | bool | If `true`, sends a message when the map changes. |
| `BotStatusEnabled` | bool | If `true`, the bot's status will be updated periodically. |
| `DisconnectDelaySeconds` | float | The delay in seconds before sending a disconnect message to handle reconnects gracefully. |
| `ChatSpamCooldownMs` | int | The cooldown in milliseconds to prevent in-game chat spam. |

### Embed Settings
| Key | Type | Description |
| --- | --- | --- |
| `UseEmbeds` | bool | If `true`, all notifications will be sent as Discord embeds. If `false`, they will be plain text. |
| `EmbedColor_Connect` | string | The hex color code (e.g., "00FF00") for connection embeds. |
| `EmbedColor_Disconnect` | string | The hex color code (e.g., "FF0000") for disconnection embeds. |
| `EmbedColor_MapChange` | string | The hex color code (e.g., "0000FF") for map change embeds. |
| `EmbedColor_Chat` | string | The hex color code (e.g., "FFFFFF") for chat message embeds. |

### Message Formatting
You can customize the format of all messages using the variables provided.

| Key | Description & Available Variables |
| --- | --- |
| `BotStatusMessage` | The bot's status message. `{MapName}`, `{PlayerCount}`, `{MaxPlayers}` |
| `MessageFormat_GameToDiscord` | In-game chat sent to Discord. `{player_name}`, `{profile_url}`, `{country_emoji}`, `{message}`, `{team_prefix}` |
| `MessageFormat_DiscordToGame` | Discord chat sent to the game. `{author_name}`, `{author_username}`, `{message}`. You can also use chat colors like `{Green}`, `{Default}`, `{White}`. |
| `MessageFormat_PlayerConnect` | Player connection message. `{player_name}`, `{profile_url}`, `{country_emoji}` |
| `MessageFormat_PlayerDisconnect` | Player disconnection message. `{player_name}`, `{profile_url}`, `{country_emoji}` |
| `MessageFormat_MapChange` | Map change message. `{map_name}` |

- This project uses the [Discord.Net](https://github.com/discord-net/Discord.Net) library
    and the [CounterStrikeSharp API](https://github.com/roflmuffin/CounterStrikeSharp).