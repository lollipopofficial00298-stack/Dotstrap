# Dotstrap

Dotstrap is a third-party replacement for the standard Roblox bootstrapper, with extra features and customization on top. It is a fork of [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap) and is **not affiliated with or endorsed by** the Bloxstrap project or Roblox Corporation.

Dotstrap only supports PCs running Windows 10 or newer.

## Features

- Discord Rich Presence showing what you're playing
- Content modding: death sound, mouse cursor (including your own image), emoji font, and more, plus backup/restore of your mods
- Server location hints (courtesy of [ipinfo.io](https://ipinfo.io)) and a server/game history with favorites
- Local playtime tracking per game (never sent anywhere)
- Engine settings: graphics API, MSAA, texture quality, and other FastFlag presets
- Customizable look: accent colors, compact launch window, custom bootstrapper themes

## Installing

Run `Dotstrap.exe`, set your preferences, and install. Once installed, Dotstrap appears in your Start Menu so you can reopen the settings.

You'll also need the [.NET 8 Desktop Runtime](https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe). You'll be asked to install it if it's missing.

Windows SmartScreen may warn you the first time you run Dotstrap because the executable isn't signed. Click "More info", then "Run anyway".

## Building

Requirements: the .NET 8 SDK (see `global.json`). The `wpfui` folder is a vendored copy of bloxstraplabs/wpfui, retargeted to .NET 8.

```
build.bat
```

This publishes a single-file `Dotstrap.exe` to the repository root.

Update checks, bug-report links and the release download button use the repository set in `App.ProjectRepository` (`Dotstrap/App.xaml.cs`). The updater runs the **first asset** of the latest GitHub release, so attach `Dotstrap.exe` as the first file and tag releases like `v2.11.5`. Analytics stay off until `App.HasAnalyticsServer` is enabled with a server of your own.

## Credits

Dotstrap is built on [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap) by pizzaboxer and contributors, licensed under the MIT License (see [LICENSE](LICENSE)). The UI uses [WPF UI](https://github.com/lepoco/wpfui), specifically the [bloxstraplabs/wpfui](https://github.com/bloxstraplabs/wpfui) fork. Many help links still point to the [Bloxstrap wiki](https://bloxstraplabs.com/wiki), since most features work the same way.
