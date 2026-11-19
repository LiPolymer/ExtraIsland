# 联动生态

ExtraIsland 无需额外依赖即可独立运行所有核心功能；同时在插件清单（`manifest.yml`）中声明了多个可选联动插件。只要你同时安装了对应插件并启用，ExtraIsland 会自动激活相关能力。

## 可选联动插件一览

| 插件名称 | 插件 ID | 联动能力 |
| --- | --- | --- |
| **LycheeLib** | `ink.lipoly.ext.lychee` | 为 [动态歌词](/components/dynamic-lyrics) 与 [当前活动](/components/live-activity) 提供歌词数据接入 |
| **SuperAutoIsland (SAI)** | `lrs2187.sai` | 自动注册 ExtraIsland 的 Blocky 积木行动、规则与数据块 |
| **IslandCaller** | `IslandCaller.Plugin2` | 注册「拉起 IslandCaller」自动化行动（实验模式下含高级版） |

---

## 详细说明

### LycheeLib（歌词服务） {#lycheelib-歌词服务}

当检测到 `ink.lipoly.ext.lychee` 已安装且启用时：

1. ExtraIsland 会在应用启动时将 `ILycheeLyrics` 服务挂载至 `Rendezvous`。
2. `动态歌词` 与 `当前活动` 会优先通过 `LycheeLib` 接收主行与副行歌词，而不再启动内置的 LyricsIsland 协议监听端口。
3. 在 [主设置](/instruction/settings) 的「歌词接口状态」中会显示 `Lychee`。

### SuperAutoIsland / SAI（积木式自动化）

当检测到 `lrs2187.sai` 已安装且启用时，ExtraIsland 会自动向 SAI 注册名为 `ExtraIsland` 的积木分组，包括：

- **行动**：`设定标志`、`语音播报`、以及（已安装 IslandCaller 时的）`拉起IslandCaller`
- **规则**：`下节课是连堂`、`当前教师是`、`下节课教师是`、`标志是`
- **数据块**：`读标志`、`获取当前值日生`、`获取名句`

详细字段与用法请参见 [自动化与 SAI 扩展](/automations/)。

### IslandCaller（随机点名）

当检测到 `IslandCaller.Plugin2` 已安装且启用时：

- 自动注册行动 **拉起IslandCaller**（`extraIsland.action.islandCaller`），触发 URI：`classisland://plugins/IslandCaller/Simple`。
- 若同时在主设置中启用了 **实验性功能**，还会额外注册 **(实验性)拉起IslandCaller-高级**（`extraIsland.action.islandCallerAdvanced`），触发 URI：`classisland://plugins/IslandCaller/Advanced/GUI`。

### 与 LyricsIsland 的兼容性说明

如果未安装 `LycheeLib`，ExtraIsland 会内置监听 `http://127.0.0.1:50063/component/lyrics/lyrics/` 以兼容 LyricsIsland 协议的歌词推送（接收 JSON 字段 `lyric` 与 `extra`）。

> [!NOTE]
> 由于歌词监听端口无法多开，当检测到 **LyricsIsland**（`jiangyin14.lyrics`）插件已启用时，ExtraIsland 会自动禁用 [当前活动](/components/live-activity) 中的「启用歌词」开关，并在主设置与组件设置中给出兼容提示。此时建议直接使用 LyricsIsland 自带的歌词组件，或配合 `LycheeLib` 使用。
