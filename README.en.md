<h1 align="center">
  <img src="./res/Icon.svg" alt="LiveSplit YS" height="46" align="top"/>
  LiveSplit YS
</h1>

<p align="center">
  <strong>A localized, polished, independently maintained LiveSplit distribution</strong>
</p>

<p align="center">
  <a href="./README.md">简体中文</a> ·
  <a href="./README.en.md">English</a>
</p>

<p align="center">
  <a href="https://github.com/Yhc-Studio/LiveSplit_YS/releases/latest">
    <img src="https://img.shields.io/github/v/release/Yhc-Studio/LiveSplit_YS?label=release" alt="Latest Release"/>
  </a>
  <a href="https://github.com/Yhc-Studio/LiveSplit_YS/blob/master/LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT License"/>
  </a>
  <a href="https://github.com/Yhc-Studio/LiveSplit_YS/issues">
    <img src="https://img.shields.io/github/issues/Yhc-Studio/LiveSplit_YS.svg" alt="GitHub Issues"/>
  </a>
  <img src="https://img.shields.io/badge/upstream-LiveSplit%201.8.37--61--gf1b471f9-555" alt="Upstream Base"/>
</p>

---

## About

**LiveSplit YS** is a customized LiveSplit distribution maintained by **Yhc_Studio**. It is intended for users who want to keep the familiar LiveSplit workflow while gaining a more complete Chinese UI, improved high-DPI rendering, smoother startup behavior, and an independent update channel.

This project is based on:

- **LiveSplit 1.8.37-61-gf1b471f9**
- Upstream revision: `f1b471f9`
- Original project: [`LiveSplit/LiveSplit`](https://github.com/LiveSplit/LiveSplit)

LiveSplit YS keeps LiveSplit's core timing, layouts, components, Auto Splitter support, and race-oriented workflows while maintaining YS-specific changes independently.

> LiveSplit YS is not an official LiveSplit release. The upstream LiveSplit project and its contributors remain independent from this project.

## LiveSplit YS Highlights

- **Chinese / English UI:** Expanded localization for the main UI, Layout Editor, component names, and commonly used settings.
- **Classic settings layout:** Component settings use the compact LiveSplit 1.8.37 layout style as their visual baseline while retaining newer component functionality.
- **High-DPI fixes:** Improved timer digit spacing, fonts, and layout behavior at scales such as 125% and 150%.
- **Faster startup and cleaner exit:** Reduced unnecessary startup scans and blocking network work, with improved cleanup of background tasks.
- **Auto Splitter improvements:** Local-cache-first behavior with online catalog refresh moved to the background.
- **Asynchronous ASL initialization:** Heavy ASL parsing / compilation work is moved away from the UI thread to reduce freezes when loading complex layouts.
- **Optional speedrun.com integration:** Automatic speedrun.com integration can be disabled independently without affecting local timing or Auto Splitters.
- **Independent LiveSplit YS update channel:** GitHub Releases and `latest.json` provide the foundation for YS-specific version checks.

## v1.0.5 Summary

- Expanded Chinese localization and component-name localization.
- Reworked component settings around the LiveSplit 1.8.37 visual style.
- Fixed high-DPI rendering and timer digit spacing issues.
- Improved startup, shutdown, AutoSplitter, and ASL loading performance.
- Added a preview of the independent LiveSplit YS update channel.

## Core LiveSplit Features

LiveSplit YS keeps the major capabilities of LiveSplit, including:

- **Accurate timing** for speedrunning with Real Time and Game Time workflows.
- **Game Time / Auto Splitting** with emulator, PC game, and script-driven timing support.
- **Multiple comparisons** such as Personal Best, Best Segments, and Sum of Best.
- **Highly customizable layouts** with reorderable components and background images.
- **Dynamic resizing** so layouts scale with the timer window.
- **Video and sound components** for media synchronized with a run.
- **Racing workflows** compatible with LiveSplit's existing race-oriented features.
- **Run sharing and split screenshots** through the existing LiveSplit workflows.
- **Component ecosystem** compatible with LiveSplit's component architecture.
- **LiveSplit Server** with Named Pipe, TCP, and WebSocket control interfaces.

## Download & Installation

Go to:

**[GitHub Releases](https://github.com/Yhc-Studio/LiveSplit_YS/releases/latest)**

Download the latest package:

```text
LiveSplitYS-vX.Y.Z.zip
```

Extract it to any writable directory and run:

```text
LiveSplit.exe
```

Before overwriting an older installation, it is recommended to back up your:

- `.lss` split files
- `.lsl` layout files
- custom components / Auto Splitters
- other personal configuration files

## Independent Update Channel

LiveSplit YS does not use the official LiveSplit application update channel. The YS stable manifest is:

```text
https://github.com/Yhc-Studio/LiveSplit_YS/releases/latest/download/latest.json
```

A stable release may contain:

```text
LiveSplitYS-v1.0.5.zip
latest.json
LiveSplitYS-v1.0.5.sha256
```

`latest.json` provides the version, download URL, SHA-256 checksum, and release notes.

> In v1.0.5, the YS update dialog is primarily a version-check / UI preview. Automatic downloading and safe in-place replacement will continue to be developed in later releases.

## Building from Source

LiveSplit YS currently targets **.NET Framework 4.8.1** and is built with a modern .NET SDK.

Requirements:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net481)
- Optional: a Visual Studio version compatible with the installed .NET SDK

Clone the repository:

```bash
git clone https://github.com/Yhc-Studio/LiveSplit_YS.git
cd LiveSplit_YS
```

Build the Release configuration:

```bash
dotnet clean LiveSplit.slnx
dotnet build LiveSplit.slnx -c Release
```

Build output is typically written to:

```text
artifacts/build/release/
```

The independent LiveSplit YS repository keeps the component sources it needs in-tree, so a normal clone does not require `--recursive` to fetch component submodules.

## Auto Splitters

LiveSplit YS remains compatible with LiveSplit Auto Splitters and the ASL workflow.

Upstream resources:

- [LiveSplit Auto Splitters](https://github.com/LiveSplit/LiveSplit.AutoSplitters)
- [LiveSplit Scriptable Auto Split](https://github.com/LiveSplit/LiveSplit.ScriptableAutoSplit)

YS-specific Auto Splitter changes focus on startup performance, local caching, and background catalog refresh without changing the normal ASL workflow.

## LiveSplit Server

The built-in LiveSplit Server allows external applications and other computers to control the timer.

Supported interfaces:

- Named Pipe: `\\<hostname>\pipe\LiveSplit`
- TCP/IP
- WebSocket: `ws://<hostname>:port/livesplit`
- Default port: `16834`

The TCP / WebSocket server must be started in LiveSplit before it can accept connections. The command protocol and compatible clients remain based on LiveSplit's existing implementation.

## Issues & Contributions

For LiveSplit YS-specific issues, please use:

- [Issues](https://github.com/Yhc-Studio/LiveSplit_YS/issues)

When reporting an issue, please include when relevant:

- LiveSplit YS version
- Windows version and DPI scale
- layout / components in use
- whether an Auto Splitter is enabled
- reproduction steps
- screenshots or related `.lsl` / `.lss` files

For general LiveSplit behavior, it can also be useful to verify whether the same issue reproduces in upstream LiveSplit.

## Upstream & Credits

LiveSplit YS is built on **LiveSplit** and its long-running component ecosystem.

- Upstream: [`LiveSplit/LiveSplit`](https://github.com/LiveSplit/LiveSplit)
- LiveSplit website: <https://livesplit.org/>
- LiveSplit YS maintainer: **Yhc_Studio**
- LiveSplit YS repository: <https://github.com/Yhc-Studio/LiveSplit_YS>

Thanks to the original LiveSplit authors, maintainers, and all contributors.

## License

This project continues to use the **MIT License**.

LiveSplit YS contains modified and integrated work based on LiveSplit. When using, redistributing, or modifying this project, preserve all applicable upstream copyright, license, and attribution notices. See [`LICENSE`](./LICENSE) for details.

---

<p align="center">
  <strong>LiveSplit YS</strong><br/>
  Maintained by Yhc_Studio
</p>
