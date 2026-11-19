# 常见问题与排查

## 动画与性能

### 为什么有些设备开启组件后略微卡顿？

[流畅时钟](/components/fluent-clock)、[更好的倒计时](/components/better-countdown)、[名句一言](/components/rhesis)、[当前活动](/components/live-activity)、[展示标志](/components/flag-display) 等组件内置了位移动画与渐变效果。在较旧设备或低功耗核显设备上：

- 可在倒计时、名句一言、当前活动、展示标志中关闭 **启用动画**。
- **流畅时钟** 强制启用动画，但可以关闭 **启用翻页动画** 或开启 **专注模式**，将高频翻页替换为轻量的微微闪烁。

---

## 功能可见性与开关

### 为什么开启「生活模式」或「实验性功能」后看不到对应组件？

在 [主设置](/instruction/settings) 中修改 **生活模式**、**实验性功能** 或 **允许遥测** 后，均需要**重启 ClassIsland**，相关组件（如 `Sleepy`、`双行容器`）与扩展行动才会在服务初始化阶段完成注册。

### 为什么「当前活动」里的「启用歌词」开关是灰色的？

当插件检测到已安装并启用 **LyricsIsland**（`jiangyin14.lyrics`）时，由于内置歌词监听接口（`127.0.0.1:50063`）无法与 LyricsIsland 共享端口，会自动禁用「当前活动」的歌词选项以防止端口冲突。

若需要同时使用歌词显示，可考虑搭配 [LycheeLib](/instruction/integrations#lycheelib-歌词服务) 使用，或直接采用 LyricsIsland 本身提供的歌词组件。

---

## 网络与外部依赖说明

- **名句一言**：请求 `v1.jinrishici.com`、`hub.saintic.com`、`v1.hitokoto.cn`、`dlystc.unknownmp.top` 等第三方公开接口；其中 `dlystc` 带有约 30 秒 CDN 缓存，短时间连续刷新可能返回重复内容。
- **值日表「自动跳过节假日」**：仅在实验模式且开启该开关时请求 `timor.tech` 节假日接口以识别法定假日与调休日。
- **Sentry 遥测**：Release 构建在「允许遥测」开启时启用 Sentry 崩溃与会话追踪；Debug 构建自动禁用。

---

## 本地构建与贡献文档

### 构建插件

仓库当前面向 `.NET 10`（`net10.0-windows`）与 `ClassIsland.PluginSdk` `2.1.1.1`：

```powershell
dotnet build -c Release
```

### 本地预览文档站点

文档基于 VitePress 构建，可在仓库根目录执行：

```powershell
npm run docs:dev
```

或构建静态产物检查链接与页面：

```powershell
npm run docs:build
```

欢迎通过 [GitHub](https://github.com/LiPolymer/ExtraIsland) 或 [GitLab](https://gitlab.com/LiPolymer/ExtraIsland) 提交 Issue 与 Pull Request 帮助完善插件与文档！
