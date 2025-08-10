# SimpleDiscordRelay for Counter-Strike 2

A simple Discord relay plugin for Counter-Strike 2 using CounterStrikeSharp.

## Features

- Relays in-game chat to a Discord channel.
- Relays Discord chat from a specific channel to the in-game chat.
- Displays notifications for player connections, disconnections, and map changes.
- Shows player's country flag next to their name.
- Player names in notifications are linked to their Steam profiles.
- Highly configurable, allowing you to enable or disable each notification type individually.

## Installation

1.  **Download the latest release:** Go to the [Releases page](https://github.com/Tsukasa-Nefren/SimpleDiscordRelay/releases/latest) of this repository and download the `SimpleDiscordRelay-vX.X.X.zip` file.
2.  **Extract:** Unzip the downloaded file. The required `GeoLite2-Country.mmdb` file is already included.
3.  **Upload Files:**
    - Create a new folder named `SimpleDiscordRelay` inside your server's `addons/counterstrikesharp/plugins` directory.
    - Upload all the extracted files into this folder.
4.  **Configure:** Edit the `config.json` file that is automatically generated after the first run to set up your Discord bot token and channel IDs.
5.  **Restart:** Restart your server or load the plugin manually.

**Note:** Remember to replace `YOUR_USERNAME/YOUR_REPOSITORY` in the link above with your actual GitHub username and repository name after you upload the project.
