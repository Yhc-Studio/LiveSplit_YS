<h1 align="center">
  <img src="./res/Icon.svg" alt="LiveSplit YS" height="46" align="top"/>
  LiveSplit YS
</h1>

<p align="center">
  <strong>基于 LiveSplit 的中文化、体验优化与长期维护版本</strong>
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

## 简介

**LiveSplit YS** 是由 **Yhc_Studio** 维护的 LiveSplit 定制版本，面向希望继续使用经典 LiveSplit 工作流，同时需要更完整中文界面、更稳定的高 DPI 显示、更顺畅的启动体验以及独立更新通道的用户。

本项目基于：

- **LiveSplit 1.8.37-61-gf1b471f9**
- Upstream revision: `f1b471f9`
- 原项目：[`LiveSplit/LiveSplit`](https://github.com/LiveSplit/LiveSplit)

LiveSplit YS 保留 LiveSplit 的核心计时、布局、组件、Auto Splitter 与比赛相关能力，并在此基础上进行独立维护。

> LiveSplit YS 不是 LiveSplit 官方发行版。原 LiveSplit 项目及其贡献者与本项目保持独立。

## LiveSplit YS 的主要改进

- **中文 / English 界面**：完善主界面、布局编辑器、组件名称与常用设置页本地化。
- **经典布局风格**：组件设置页面以 LiveSplit 1.8.37 的紧凑布局为视觉基线，同时保留新版组件功能。
- **高 DPI 修复**：改善 125% / 150% 等缩放下的计时数字间距、字体与布局表现。
- **更快的启动与退出**：减少启动阶段不必要的组件扫描和网络阻塞，并完善后台任务清理。
- **Auto Splitter 优化**：优先使用本地缓存，在线目录刷新放到后台执行，降低联网对启动的影响。
- **ASL 异步初始化**：将较重的 ASL 解析 / 编译移出 UI 主线程，减少打开复杂布局时的卡顿。
- **可选 speedrun.com 集成**：自动 speedrun.com 联网可以独立关闭，不影响本地计时和 Auto Splitter 使用。
- **LiveSplit YS 独立更新通道**：使用 GitHub Releases 与 `latest.json` 作为 YS 自己的版本检查基础。

## v1.0.5 更新摘要

- 完善中文界面与组件名称本地化。
- 按 LiveSplit 1.8.37 风格优化组件设置页布局。
- 修复高 DPI 显示及计时文字间距问题。
- 优化启动、退出、AutoSplitter 与 ASL 脚本加载性能。
- 新增 LiveSplit YS 独立更新通道预览。

## LiveSplit 核心功能

LiveSplit YS 保留并继续兼容 LiveSplit 的主要能力：

- **精确计时**：适用于 Speedrun 的实时计时与游戏时间计时。
- **Game Time / Auto Splitting**：支持游戏时间读取与自动分段脚本。
- **多种比较方式**：支持 PB、Best Segments、Sum of Best 等比较方式。
- **高度可定制的布局系统**：自由添加、删除、排序与配置组件，并支持背景图片。
- **动态缩放**：窗口改变尺寸时，布局可以随之缩放。
- **视频与音效组件**：可在计时过程中联动视频、音效等媒体组件。
- **比赛功能**：继续兼容 LiveSplit 原有的比赛与计时工作流。
- **分享与截图**：保留 LiveSplit 原有的成绩、分段截图与相关分享能力。
- **组件系统**：继续支持 LiveSplit 的组件架构与第三方组件生态。
- **LiveSplit Server**：支持 Named Pipe、TCP 与 WebSocket 控制接口。

## 下载与安装

前往：

**[GitHub Releases](https://github.com/Yhc-Studio/LiveSplit_YS/releases/latest)**

下载最新的：

```text
LiveSplitYS-vX.Y.Z.zip
```

解压到任意可写目录后直接运行：

```text
LiveSplit.exe
```

建议在覆盖旧版本前备份自己的：

- `.lss` 分段文件
- `.lsl` 布局文件
- 自定义组件 / Auto Splitter
- 其他个人配置文件

## 独立更新通道

LiveSplit YS 不使用 LiveSplit 官方主程序更新通道。YS 的稳定版更新清单位于：

```text
https://github.com/Yhc-Studio/LiveSplit_YS/releases/latest/download/latest.json
```

稳定版 Release 可包含：

```text
LiveSplitYS-v1.0.5.zip
latest.json
LiveSplitYS-v1.0.5.sha256
```

`latest.json` 用于提供版本号、下载地址、SHA-256 与更新说明。

> v1.0.5 中更新窗口仍以版本检查 / 界面预览为主；自动下载与安全替换流程将由后续版本继续完善。

## 从源码编译

LiveSplit YS 当前使用 **.NET Framework 4.8.1**，并通过现代 .NET SDK 构建。

需要安装：

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net481)
- 可选：与所安装 .NET SDK 兼容的 Visual Studio

克隆仓库：

```bash
git clone https://github.com/Yhc-Studio/LiveSplit_YS.git
cd LiveSplit_YS
```

构建 Release：

```bash
dotnet clean LiveSplit.slnx
dotnet build LiveSplit.slnx -c Release
```

构建输出通常位于：

```text
artifacts/build/release/
```

LiveSplit YS 的独立仓库将所需组件源码直接纳入项目维护，因此正常克隆本仓库时不需要额外使用 `--recursive` 拉取组件子模块。

## Auto Splitters

LiveSplit YS 继续兼容 LiveSplit 的 Auto Splitter / ASL 工作流。

Auto Splitter 相关资料可参考 LiveSplit 上游文档：

- [LiveSplit Auto Splitters](https://github.com/LiveSplit/LiveSplit.AutoSplitters)
- [LiveSplit Scriptable Auto Split](https://github.com/LiveSplit/LiveSplit.ScriptableAutoSplit)

YS 对 Auto Splitter 的主要调整集中在启动性能、目录缓存和后台刷新，不改变现有 ASL 使用习惯。

## LiveSplit Server

LiveSplit 内置 Server 可让其他程序或其他计算机控制计时器。

支持：

- Named Pipe：`\\<hostname>\pipe\LiveSplit`
- TCP/IP
- WebSocket：`ws://<hostname>:port/livesplit`
- 默认端口：`16834`

TCP / WebSocket Server 需要在 LiveSplit 中启动后才能使用。具体命令协议与兼容客户端仍遵循 LiveSplit 原有实现。

## 问题反馈与贡献

如果发现 LiveSplit YS 自身的问题，请在本仓库提交：

- [Issues](https://github.com/Yhc-Studio/LiveSplit_YS/issues)

提交问题时建议附上：

- LiveSplit YS 版本
- Windows 版本及 DPI 缩放比例
- 使用的布局 / 组件
- 是否启用 Auto Splitter
- 可以复现问题的步骤
- 必要时附截图或相关 `.lsl` / `.lss`

对于来自 LiveSplit 上游的通用问题，也建议先确认问题是否能在官方 LiveSplit 中复现。

## 上游与致谢

LiveSplit YS 建立在 **LiveSplit** 及其长期维护的组件生态之上。

- Upstream：[`LiveSplit/LiveSplit`](https://github.com/LiveSplit/LiveSplit)
- LiveSplit 官网：<https://livesplit.org/>
- LiveSplit YS 维护者：**Yhc_Studio**
- LiveSplit YS 仓库：<https://github.com/Yhc-Studio/LiveSplit_YS>

感谢 LiveSplit 原作者、维护者与所有贡献者。

## License

本项目继续遵循 **MIT License**。

LiveSplit YS 包含基于 LiveSplit 的修改与整合内容。使用、分发和二次开发时，请同时保留适用的原项目版权、许可证与归属信息。详见仓库中的 [`LICENSE`](./LICENSE)。

---

<p align="center">
  <strong>LiveSplit YS</strong><br/>
  Maintained by Yhc_Studio
</p>
