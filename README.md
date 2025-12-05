<!--markdownlint-disable MD033 MD036-->

<div align="center">

<img src="./ExtraIsland/Assets/fullLogo.svg" alt="ExtraIsland Logo" style="height: 100px;">

# ExtraIsland

**✨「课岛进化」** ✨

[![GitHub](https://img.shields.io/badge/GitHub-%23121011.svg?logo=github&logoColor=white)](https://github.com/LiPolymer/ExtraIsland)
[![GitLab](https://img.shields.io/badge/GitLab-FC6D26?logo=gitlab&logoColor=fff)](https://gitlab.com/LiPolymer/ExtraIsland)

> 本分支适用于ClassIsland v2

![Repobeatsa](https://repobeats.axiom.co/api/embed/1f18128f350eea1c2612fe115498942e5c4fefff.svg "Repobeats Analytics Image")

[ExtraIsland](https://docs.lipoly.ink/ExtraIsland) 是一款 [ClassIsland](https://classisland.tech/) 的实用插件，为 ClassIsland 增加了更好的倒计日、流畅时钟、名句一言、值日表、换届导引、Sleepy 等组件和功能！

![GitHub Commit Activity](https://img.shields.io/github/commit-activity/t/LiPolymer/ExtraIsland)
![GitHub Last Commit](https://img.shields.io/github/last-commit/LiPolymer/ExtraIsland)
![GitHub Commits Since Latest Release](https://img.shields.io/github/commits-since/LiPolymer/ExtraIsland/latest)
![GitHub Created At](https://img.shields.io/github/created-at/LiPolymer/ExtraIsland)
![GitHub Release Date](https://img.shields.io/github/release-date-pre/LiPolymer/ExtraIsland)
![GitHub Release](https://img.shields.io/github/v/release/LiPolymer/ExtraIsland?include_prereleases)
![GitHub Language Count](https://img.shields.io/github/languages/count/LiPolymer/ExtraIsland)
![GitHub Top Language](https://img.shields.io/github/languages/top/LiPolymer/ExtraIsland)
![GitHub Repo Size](https://img.shields.io/github/repo-size/LiPolymer/ExtraIsland)
![GitHub License](https://img.shields.io/github/license/LiPolymer/ExtraIsland)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/LiPolymer/ExtraIsland/total)
[![Visitors](https://api.visitorbadge.io/api/visitors?path=https%3A%2F%2Fgithub.com%2FLiPolymer%2FExtraIsland&label=visits&countColor=%2337d67a&style=flat)](https://visitorbadge.io/status?path=https%3A%2F%2Fgithub.com%2FLiPolymer%2FExtraIsland)
![Website](https://img.shields.io/website?url=https%3A%2F%2Fdocs.lipoly.ink%2FExtraIsland)

![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/LiPolymer/ExtraIsland)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-closed/LiPolymer/ExtraIsland)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr/LiPolymer/ExtraIsland)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr-closed/LiPolymer/ExtraIsland)
![GitHub Discussions](https://img.shields.io/github/discussions/LiPolymer/ExtraIsland)
![GitHub Number of Milestones](https://img.shields.io/github/milestones/all/LiPolymer/ExtraIsland)
![GitHub Forks](https://img.shields.io/github/forks/LiPolymer/ExtraIsland)
![GitHub Repo Stars](https://img.shields.io/github/stars/LiPolymer/ExtraIsland)
![GitHub Watchers](https://img.shields.io/github/watchers/LiPolymer/ExtraIsland)

![WakaTime](https://wakatime.com/badge/user/0c9f5a71-56d1-4ba3-b95c-da9e563fa119/project/c6f9dda0-5282-4b6b-b0e1-4461c06a5b41.svg?style=flat)

</div>

为 [ClassIsland](https://classisland.tech/) 扩充一些小小的功能

🧐 最新 commit 尝鲜可以前往 [GitLab Pipeline](https://gitlab.com/LiPolymer/ExtraIsland/-/pipelines) 获取 [未打包](https://docs.classisland.tech/dev/plugins/publishing.html#%E6%89%93%E5%8C%85%E6%8F%92%E4%BB%B6) 的构建

_持续开发中_

本项目处于起步阶段 ☝🤓

欢迎大家在 Issues 提交**功能请求**,帮我把这个项目变得更好!🤗

> [!TIP]
>
> 由于时间安排困难,很多细节没有打磨
> 尤其是文档(wiki)
> 如果您愿意为我们丰富一下支持内容,
> 欢迎并感谢您为我们创建PR! 😋🙏

_~~其实想直接贡献主程序的 但感觉我代码质量太低了 就写成了插件~~_

_也算是以前写的班级小工具的精神续作(?_

> [!IMPORTANT]
>
> 本插件的部分组件应用了动画效果。动画效果在部分较旧的设备上可能引发性能问题，请用户酌情启用插件的动画效果!
>
> **强制使用动画效果的组件**：`流畅时钟`.
>

<div align="center">

**感谢同学们对本项目的支持与贡献！**

[![Contributors](https://contrib.nn.ci/api?repo=LiPolymer/ExtraIsland&repo=LiPolymer/SentenceSplicer)](https://github.com/LiPolymer/ExtraIsland/graphs/contributors)

</div>

感谢 `Aris` 提供 `当前活动` 组件的UI设计原稿!

## 完成的功能
### 组件

流畅时钟

<div align="center">

![流畅时钟组件截图](./assets/README_screenshots/fluent_clock.png)

</div>

- 时钟有切换和闪动动画

- 可关闭秒数显示，或使用小字体显示秒数

- 从准确时间服务获取时间，或获取 Windows 时间

- 整点强调动画

更好的倒计日

<div align="center">

![更好的倒计日组件截图](./assets/README_screenshots/better_countdown.png)

</div>

- 倒计时可精确到秒

- 支持自动隐藏为零的节点

- 带有切换动画(支持关闭)

- 可配置倒计时文本

- 从准确时间服务获取时间，或获取 Windows 时间

- 更好的配置面板

值日表

<div align="center">

![值日表设置页面截图](./assets/README_screenshots/duty_student.png)

</div>

- 组件支持紧凑模式

- 三种轮换方式

- 轮换间隔设定

- 简便的名单管理，可从文本文档中导入值日生名单

- 自动整理索引排序

- (实验性) 自动循环

名句一言

<div align="center">

![名句一言组件截图](./assets/README_screenshots/hitokoto.png)

</div>

- 支持三个 API，且 API 可配置

- 自动刷新

- 自定义请求

- 支持排除列表

- (早期) 字数限制

- 带有切换或闪动动画(可关闭)

当前活动

- 显示当前焦点所在的窗口名

- 显示歌词(使用 LyricsIsland 协议)

- 支持正则替换列表

- 支持Sleepy上传(需启用生活模式)

### 自动化
触发器
- 间隔触发 `每过一段时间触发!`

规则
- 今天是... `判断今天是周几/周内/周末`
- 时间晚于...
- 读标志 `读取由设标志行动设定的标志并比较!`

行动
- 设标志 `设定一个标志以供规则读取`
- 拉起IslandCaller `静默唤起IslandCaller进行抽取!(需要安装IslandCaller)`

### 其他
微功能

  - 导引界面 `为接手智能白板的下一届同学们留下 ClassIsland 等工具的使用导引`

生活模式 `用于容纳那些不适用于课堂的功能`:

  - Sleepy 组件 ~~`随地视奸`~~ `连接到现有的Sleepy实例,并在岛上显示!`

    - 支持轮播设备状态
  
    - 带有切换动画(可配置)

## 正在开发(计划中/未实装/未完成)
- 弹出式提醒
- 自动化:
- - 行动:
- - - 主窗口操作器
- 名句一言:
- - 本地语句源
- - 展示作者/出处