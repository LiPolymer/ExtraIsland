# 自动化与 SAI 扩展

ExtraIsland 为 ClassIsland 自动化引擎补充了触发器、规则与行动，同时全面兼容 **SuperAutoIsland (SAI)**，自动注册对应的积木块与数据块。

## 触发器

### 间隔触发（`extraIsland.trigger.timePassed`）

每隔固定时间间隔触发一次自动化。

- **配置项**：间隔秒数（`TimeGapSeconds`，默认 `120` 秒）。
- **机制**：基于 ClassIsland 主计时器周期检测距上次触发经过的时间，超过设定间隔即再次触发。

---

## 规则

### 今天是（`extraIsland.rule.todayIs`）

判断当前星期属性：

- **工作日**（周一至周五）
- **周末**（周六、周日）
- **星期...**（指定星期一至星期日中的某一天）

### 时间晚于（`extraIsland.rule.laterThan`）

判断当前本地精确时间（`TimeOfDay`）是否已到达或晚于设定的目标时刻（`>= 目标时刻` 时返回 `true`）。

### 当前教师是 / 下节课教师是

- **当前教师是**（`extraIsland.rule.currentTeacherIs`）：精确匹配当前正在进行的课程的授课教师姓名。
- **下节课教师是**（`extraIsland.rule.nextTeacherIs`）：精确匹配下一节课程的授课教师姓名。

### 下节课连堂（`extraIsland.rule.isDoubleLesson`）

判断下一节课是否与当前课程（上课中时取当前课程；课间/准备上课时取刚结束的上一节课）属于同一科目（`SubjectId` 一致）。

### 读标志（`extraIsland.rule.flagIs`）

读取指定 ID 的标志值，判断其是否与期望内容完全相等。

- **读取优先级**：内存标志优先于持久化标志，若标志尚未设定则匹配失败。
- **常见搭配**：配合 [设标志](#设标志) 行动与 [展示标志](/components/flag-display) 组件实现跨流程状态控制。

### 存在最大化/全屏窗口（`extraIsland.rule.windowStatus`） *(仅 Windows)*

检测当前系统中是否存在处于**最大化**或**全屏**状态的可见窗口（会自动过滤隐藏/Cloaked 窗口及桌面容器窗口 `WorkerW`、`Progman`）。

- **可选项**：`最大化` / `全屏`
- **实时响应**：插件内置窗口状态监听器 (`WindowStatusWatcher`)，可在窗口状态变化时通知规则集更新。

---

## 行动

### 设标志（`extraIsland.action.setFlag`） {#设标志}

设置一个键值对标志（`TargetFlag` 为 `FlagContent`），支持在自动化恢复时自动撤销该标志。

- **持久化**：勾选后标志会保存至插件持久化配置文件（`Persisted/Flag.json`），重启 ClassIsland 后仍然保留；未勾选时仅写入内存，重启清空。
- **更新规则**：在执行或恢复后通知 ClassIsland 规则集刷新判断（默认勾选）。

### 更新规则集（`extraIsland.action.updateRule`）

主动调用 `RulesetService.NotifyStatusChanged()` 刷新当前全部规则状态，适用于需要手动触发规则重算的场景。

### 语音播报（`extraIsland.action.doSpeech`）

将指定文本加入 ClassIsland 语音服务（`ISpeechService`）播报队列。

> [!NOTE]
> 要播报的文本不能为空或纯空白字符，否则执行行动时会报错提示。

### 发送值日提示（`extraIsland.action.dutyNotify`）

读取全局 [值日表](/components/on-duty) 中的当前值日生与下一次轮换人员，通过 ExtraIsland 通知提供方发送岛内提醒。

- **内容格式模板**：默认为 `当前值日:{0} · 下次轮换:{1}`，其中 `{0}` 为当前值日生，`{1}` 为下次轮换人员（若无下次轮换则显示 `无`）。
- **通知置顶**：开启后以置顶级别弹出提醒。
- **等待提醒完成**：勾选后行动会等待提醒播报/展示结束后再结束。

### 拉起 IslandCaller *(需安装 IslandCaller)*

当检测到已安装并启用 `IslandCaller.Plugin2` 时自动注册：

- **拉起IslandCaller**（`extraIsland.action.islandCaller`）：静默调用 `classisland://plugins/IslandCaller/Simple` 进行抽取。
- **(实验性)拉起IslandCaller-高级**（`extraIsland.action.islandCallerAdvanced`）：仅在开启 [实验性功能](/instruction/settings#实验性功能) 后注册，调用 `classisland://plugins/IslandCaller/Advanced/GUI`。

---

## SuperAutoIsland (SAI) 扩展与数据块

当检测到已安装并启用 **SuperAutoIsland**（`lrs2187.sai`）时，ExtraIsland 会自动注册名为 `ExtraIsland` 的积木分类：

### SAI 积木列表

- **行动**：`设定标志`、`语音播报`，以及（已安装 IslandCaller 时的）`拉起IslandCaller` / `(实验性)拉起IslandCaller-高级`
- **规则**：`下节课是连堂`、`当前教师是`、`下节课教师是`、`标志是`
- **数据块**：

| 数据块 ID | 名称 | 输出类型 | 说明 |
| --- | --- | --- | --- |
| `extraIsland.data.getFlag` | **读标志** | `String` | 读取指定 `TargetFlag` 的内容（内存优先于持久化），未设置时返回 `[未设置值]` |
| `extraIsland.data.getOnDuty` | **获取当前值日生** | `String` | 直接输出当前值日生文本 |
| `extraIsland.data.getRhesis` | **获取名句** | `String` | 支持独立配置 `一言`（权重+查询参数）、`今日诗词`（权重）、`每日一句`（权重）、`诏预`（权重+接口路径）、字数限制与回车分隔的排除列表，抓取并过滤后返回名句正文 |

---

## 实用组合示例

::: tip 示例 1：状态标志联动控制
1. 在特定自动化触发时运行 **设标志**，将 `mode` 设为 `自习`（可按需勾选持久化）。
2. 在组件方案中使用 [展示标志](/components/flag-display) 组件，将「标志 ID」填为 `mode`、占位文本设为 `正常上课`。
3. 在其他规则中使用 **读标志** 判断 `mode` 是否是 `自习`，实现跨流程联动切换组件方案或触发播报。
:::

::: tip 示例 2：定时值日提醒 / 连堂语音提示
- 搭配 **间隔触发** 或上课/放学触发器，调用 **发送值日提示**，每天放学前自动提醒值日生与明日接班同学。
- 使用 **下节课连堂** 规则配合 **语音播报**，在课间提前提示同学们“下节课为连堂课，请提前做好准备”。
:::
