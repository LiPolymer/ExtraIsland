using System.Threading;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Platforms.Abstraction.Models;
using ClassIsland.Shared;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Shared;

public static class WindowStatusWatcher {
    const int IntervalMilliseconds = 1000;
    static Timer? _timer;
    static IWindowRuleService? _windowRuleService;
    static int _lastState = -1;

    public static void Start() {
        if (_timer != null) return;
        try {
            _windowRuleService = IAppHost.TryGetService<IWindowRuleService>();
            if (_windowRuleService != null) {
                _windowRuleService.ForegroundWindowChanged += OnForegroundWindowChanged;
            }
        } catch (Exception e) {
            GlobalConstants.HostInterfaces.PluginLogger?.LogWarning(e, "无法订阅前台窗口变化事件，将仅依赖轮询检测窗口状态变化");
        }
        _lastState = -1;
        _timer = new Timer(_ => Evaluate(), null, IntervalMilliseconds, IntervalMilliseconds);
    }

    public static void Stop() {
        if (_windowRuleService != null) {
            _windowRuleService.ForegroundWindowChanged -= OnForegroundWindowChanged;
            _windowRuleService = null;
        }
        _timer?.Dispose();
        _timer = null;
    }

    static void OnForegroundWindowChanged(object? sender, ForegroundWindowChangedEventArgs e) {
        Evaluate();
    }

    static void Evaluate() {
        try {
            int state = WindowStatusDetect.Snapshot();
            int last = Interlocked.Exchange(ref _lastState, state);
            if (last != -1 && last != state) {
                GlobalConstants.HostInterfaces.RulesetService?.NotifyStatusChanged();
            }
        } catch (Exception e) {
            GlobalConstants.HostInterfaces.PluginLogger?.LogDebug(e, "检测窗口最大化/全屏状态失败");
        }
    }
}
