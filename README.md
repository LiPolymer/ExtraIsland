<!--markdownlint-disable MD033 MD036-->

<div align="center">

<img src="./ExtraIsland/Assets/fullLogo.svg" alt="ExtraIsland Logo" height="100">

# ExtraIsland

**✨「课岛进化」✨**

[使用文档](https://docs.lipoly.ink/ExtraIsland) · [快速开始](https://docs.lipoly.ink/ExtraIsland/instruction/gettingStarted.html) · [讨论社区](https://docs.lipoly.ink/ExtraIsland/information/community.html) · [更新日志 / Releases](https://github.com/LiPolymer/ExtraIsland/releases)

[![GitHub](https://img.shields.io/badge/GitHub-%23121011.svg?logo=github&logoColor=white)](https://github.com/LiPolymer/ExtraIsland)
[![GitLab](https://img.shields.io/badge/GitLab-FC6D26?logo=gitlab&logoColor=fff)](https://gitlab.com/LiPolymer/ExtraIsland)
[![GitHub Release](https://img.shields.io/github/v/release/LiPolymer/ExtraIsland?include_prereleases)](https://github.com/LiPolymer/ExtraIsland/releases)
[![GitHub Downloads](https://img.shields.io/github/downloads/LiPolymer/ExtraIsland/total)](https://github.com/LiPolymer/ExtraIsland/releases)
[![GitHub License](https://img.shields.io/github/license/LiPolymer/ExtraIsland)](./LICENSE.txt)
[![GitHub Repo Stars](https://img.shields.io/github/stars/LiPolymer/ExtraIsland)](https://github.com/LiPolymer/ExtraIsland/stargazers)
[![GitHub Last Commit](https://img.shields.io/github/last-commit/LiPolymer/ExtraIsland)](https://github.com/LiPolymer/ExtraIsland/commits/v1)
[![Website](https://img.shields.io/website?url=https%3A%2F%2Fdocs.lipoly.ink%2FExtraIsland)](https://docs.lipoly.ink/ExtraIsland)
[![Visitors](https://api.visitorbadge.io/api/visitors?path=https%3A%2F%2Fgithub.com%2FLiPolymer%2FExtraIsland&label=visits&countColor=%2337d67a&style=flat)](https://visitorbadge.io/status?path=https%3A%2F%2Fgithub.com%2FLiPolymer%2FExtraIsland)
![WakaTime](https://wakatime.com/badge/user/0c9f5a71-56d1-4ba3-b95c-da9e563fa119/project/c6f9dda0-5282-4b6b-b0e1-4461c06a5b41.svg?style=flat)

> 本分支 (`v1`) 适用于 **ClassIsland v2**，已正式上架 ClassIsland 插件市场！

[ExtraIsland](https://docs.lipoly.ink/ExtraIsland) 是一款 [ClassIsland](https://classisland.tech/) 插件，提供更丰富的实用组件、自动化扩展（兼容 SAI）与访问验证等功能。

<img src="https://lipoly.ink/assets/badges/humanity.svg" alt="This project was built by human" width="200">

*~~算是吧...~~*

![Repobeats](https://repobeats.axiom.co/api/embed/1f18128f350eea1c2612fe115498942e5c4fefff.svg "Repobeats Analytics Image")

</div>

---

_持续开发中，欢迎在 [Issues](https://github.com/LiPolymer/ExtraIsland/issues) 提交 **功能请求** 或反馈问题，一起把这个项目变得更好！🤗_

_~~其实想直接贡献主程序的，但感觉代码质量太低了就写成了插件~~_  
_也算是以前写的班级小工具的精神续作（？_

> [!IMPORTANT]
> 本插件的部分组件应用了动画效果。动画效果在部分较旧的设备上可能引发性能问题，请酌情在各组件设置中关闭动画效果。  
> **强制使用动画效果的组件**：`流畅时钟`。

> [!TIP]
> 由于时间安排有限，文档与部分细节仍在持续完善中。如果你愿意帮忙补充文档或改进代码，非常欢迎提交 PR！😋🙏

## 安装方式

### 插件市场安装（推荐）

打开 ClassIsland 设置 → `插件` → `插件市场`，搜索 **ExtraIsland** 直接安装即可。

### 手动安装

1. 前往 [GitHub Releases](https://github.com/LiPolymer/ExtraIsland/releases) 或 [GitLab Releases](https://gitlab.com/LiPolymer/ExtraIsland/-/releases) 下载最新的 `*.cipx` 安装包。
2. 打开 ClassIsland 设置 → `插件`，点击菜单中的 `从本地安装...`，选择下载的 `.cipx` 文件。

> 🧐 最新 commit 尝鲜构建可前往 [GitLab Pipelines](https://gitlab.com/LiPolymer/ExtraIsland/-/pipelines) 获取，打包与安装步骤详见 [快速开始文档](https://docs.lipoly.ink/ExtraIsland/instruction/gettingStarted.html)。

### 可选联动插件

ExtraIsland 无需额外依赖即可独立使用；若安装了以下插件，将自动解锁对应的联动功能：

- **LycheeLib** (`ink.lipoly.ext.lychee`)：为 `动态歌词` 与 `当前活动` 提供歌词接口接入。
- **SuperAutoIsland (SAI)** (`lrs2187.sai`)：自动注册 ExtraIsland 的积木式行动、规则与数据块。
- **IslandCaller** (`IslandCaller.Plugin2`)：解锁「拉起 IslandCaller」自动化行动。

## 功能一览

### 组件

- **流畅时钟 (`FluentClock`)**
  - 数字切换与冒号闪动动画、整点强调动画
  - 支持关闭秒数显示，或以小字体显示秒数
  - 支持从准确时间服务或系统时间获取时间
- **更好的倒计时 (`BetterCountdown`)**
  - 倒计时精确到秒，支持自动隐藏为零的时间单位
  - 支持切换动画（可关闭）与自定义前后文案
  - 支持从准确时间服务或系统时间获取时间
  - 倒计时结束时发送提醒
- **值日表 (`OnDuty`)** *(含独立设置页面)*
  - 支持紧凑模式与自动循环
  - 支持三种轮换方式与自定义轮换间隔
  - 便捷的名单管理：支持从文本文件导入（一行一个）与自动整理索引排序
  - *(实验性)* 自动跳过节假日
- **名句一言 (`Rhesis`)**
  - 内置四个可配置内容源：**今日诗词**、**诏预 (Saintic)**、**一言 (Hitokoto)**、**每日一句 (dlystc)**，支持按概率权重与来源参数灵活配置
  - 支持展示作者与出处，并提供多种额外信息展示方式
  - 支持自动刷新、字数限制与排除列表（支持从文本文件导入）
  - 支持切换 / 翻页动画（可关闭）
- **当前活动 (`LiveActivity`)**
  - 展示当前焦点窗口标题，支持正则替换列表
  - 支持歌词显示（兼容 LyricsIsland 协议 / LycheeLib）
  - 支持 Sleepy 状态上传（需启用生活模式）
- **动态歌词 (`DynamicLyrics`)**
  - 独立歌词展示组件，支持主 / 副行歌词切换与切换动画
  - 兼容 LyricsIsland 协议与 LycheeLib
- **档案信息 (`ProfileInformation`)**
  - 展示当前学期周次（如「第 N 周」）或单双周状态
  - 支持简洁模式与自定义每周起始日
- **展示标志 (`FlagDisplay`)**
  - 实时展示由自动化「设标志」行动设定的标志内容，带切换动画
- **双行容器 (`DualLineContainer`)** *(实验性功能)*
  - 容器组件，可将多个子组件自动分为上下两小行紧凑排布

### 认证与管理

- **U 盘验证 (`UsbDriveAuthorizer`)** *(仅 Windows)*
  - 使用 U 盘作为访问凭据对 ClassIsland 进行防护，支持硬件序列号 (S/N) 或密钥文件两种验证模式

### 自动化 *(兼容 SuperAutoIsland / SAI)*

- **触发器**
  - `间隔触发`：每隔设定时间触发一次
- **规则**
  - `今天是`：判断今天是周几 / 周内 / 周末
  - `时间晚于`：判断当前时间是否晚于指定时刻
  - `当前教师是` / `下节课教师是`：按授课教师进行匹配
  - `下节课连堂`：判断下节课是否与当前课程连堂
  - `读标志`：读取由「设标志」行动设定的标志并进行比较
  - `存在最大化/全屏窗口` *(仅 Windows)*：检测当前是否存在最大化或全屏窗口
- **行动**
  - `设标志`：设定内存或持久化标志，并可选立即更新规则集
  - `更新规则集`：手动刷新规则集状态
  - `语音播报`：调用 ClassIsland 语音服务播报指定文本
  - `发送值日提示`：发送包含当前与下次轮换值日生的自定义格式提醒
  - `拉起 IslandCaller`：静默唤起 IslandCaller 进行抽取（需安装 IslandCaller；实验模式下额外提供高级版）
- **SAI 数据块** *(需安装 SuperAutoIsland)*
  - `读标志` (`GetFlag`)、`获取当前值日生` (`GetOnDuty`)、`获取名句` (`GetRhesis`)

### 生活模式

在插件主设置中开启 **生活模式** 后，将启用面向个人/非课堂场景的功能：

- **Sleepy 组件**：连接到现有 Sleepy 实例并在岛上展示状态，支持自动刷新、多设备轮播与切换动画

## 致谢

<div align="center">

**感谢同学们对本项目的支持与贡献！**

[![Contributors](https://contrib.nn.ci/api?repo=LiPolymer/ExtraIsland&repo=LiPolymer/SentenceSplicer)](https://github.com/LiPolymer/ExtraIsland/graphs/contributors)

</div>

- 感谢 [Aris](https://github.com/Aris-Offline) 提供 `当前活动` 组件的 UI 设计原稿！
- 感谢所有提交 Issue、PR 与参与测试反馈的社区成员！

## 开源协议

本项目基于 [GNU Affero General Public License v3.0 (AGPLv3)](./LICENSE.txt) 开源。
