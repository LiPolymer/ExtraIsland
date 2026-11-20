using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
[ComponentInfo(
                  "0EA67B3B-E4CB-56C1-AFDC-F3EA7F38924D",
                  "流畅时钟",
                  "\uE4D2",
                  "拥有动画支持"
              )]
public partial class FluentClock : ComponentBase<FluentClockConfig> {
    /// <summary>
    /// 整点强调完整时长 (淡入 + 停留 + 淡出), 期间忽略重复触发
    /// </summary>
    const long EmphasizeDurationMs = 3400;

    public FluentClock(ILessonsService lessonsService,IExactTimeService exactTimeService) {
        ExactTimeService = exactTimeService;
        LessonsService = lessonsService;
        InitializeComponent();
        _digits = [
            new DigitSlot(LHourTens),
            new DigitSlot(LHourUnits),
            new DigitSlot(LMinTens),
            new DigitSlot(LMinUnits),
            new DigitSlot(LSecTens),
            new DigitSlot(LSecUnits)
        ];
        _separatorAnimator = new Animators.SeparatorVisualAnimator(SMins);
        _emphasizeAnimator = new Animators.EmphasizerVisualAnimator(EmpBack);
    }

    IExactTimeService ExactTimeService { get; }
    ILessonsService LessonsService { get; }

    readonly DigitSlot[] _digits;
    readonly Animators.SeparatorVisualAnimator _separatorAnimator;
    readonly Animators.EmphasizerVisualAnimator _emphasizeAnimator;

    /// <summary>
    /// 上一次渲染的帧, 唯一的显示状态来源; 为 null 时下一帧将静默渲染
    /// </summary>
    ClockFrame? _lastFrame;
    bool _separatorVisible = true;
    long _emphasizeUntil;

    int _attachGeneration;
    FluentClockConfig? _subscribedSettings;

    #region Lifecycle

    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        int generation = ++_attachGeneration;
        Dispatcher.UIThread.Post(() => {
            if (generation != _attachGeneration) return;
            Start();
        });
    }

    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        ++_attachGeneration;
        Stop();
    }

    void Start() {
        Stop();
        _subscribedSettings = Settings;
        _subscribedSettings.PropertyChanged += OnSettingsChanged;
        LessonsService.PostMainTimerTicked += OnMainTimerTicked;
        RootPanel.SizeChanged += OnRootPanelSizeChanged;
        _lastFrame = null;
        ApplyLayout();
        Tick();
        SyncBackgroundWidth();
    }

    void Stop() {
        if (_subscribedSettings == null) return;
        _subscribedSettings.PropertyChanged -= OnSettingsChanged;
        LessonsService.PostMainTimerTicked -= OnMainTimerTicked;
        RootPanel.SizeChanged -= OnRootPanelSizeChanged;
        _subscribedSettings = null;
    }

    #endregion

    #region Event entries (统一切换到 UI 线程)

    void OnMainTimerTicked(object? sender,EventArgs e) {
        RunOnUiThread(Tick);
    }

    void OnSettingsChanged(object? sender,PropertyChangedEventArgs e) {
        RunOnUiThread(() => {
            if (_subscribedSettings == null) return;
            switch (e.PropertyName) {
                case nameof(FluentClockConfig.IsAccurate):
                    ApplyLayout();
                    Tick();
                    break;
                case nameof(FluentClockConfig.HorizontalGap):
                    ApplyLayout();
                    break;
                case nameof(FluentClockConfig.IsSystemTime):
                    Tick();
                    break;
                case nameof(FluentClockConfig.IsOClockEmp):
                    // 开启时预览一次强调特效
                    if (Settings.IsOClockEmp) TryEmphasize();
                    break;
            }
        });
    }

    void OnRootPanelSizeChanged(object? sender,SizeChangedEventArgs e) {
        SyncBackgroundWidth();
    }

    static void RunOnUiThread(Action action) {
        if (Dispatcher.UIThread.CheckAccess()) {
            action();
        } else {
            Dispatcher.UIThread.Post(action);
        }
    }

    #endregion

    #region Rendering

    void Tick() {
        if (_subscribedSettings == null) return;
        DateTime now = Settings.IsSystemTime
            ? DateTime.Now
            : ExactTimeService.GetCurrentLocalDateTime();
        Render(ClockFrame.From(now,Settings.IsAccurate));
    }

    /// <summary>
    /// 单一渲染路径: 与上一帧比较, 变化的位播放动画, 未变化的位自愈 (修正卡顿/丢帧导致的不同步)
    /// </summary>
    void Render(ClockFrame frame) {
        ClockFrame? prev = _lastFrame;
        _lastFrame = frame;

        bool swapEnabled = Settings.IsSwapAnimationEnabled;
        bool secondsSwap = prev is { } p
                           && ClockFrame.ShouldSwapSeconds(p,frame,swapEnabled,Settings.IsFocusedMode);

        for (int i = 0; i < ClockFrame.DigitCount; i++) {
            char digit = frame.DigitAt(i);
            bool isSecond = ClockFrame.IsSecondDigit(i);
            bool changed = prev is { } pf && pf.DigitAt(i) != digit;
            if (changed && (!isSecond || frame.ShowSeconds)) {
                _digits[i].Set(digit,isSecond ? secondsSwap : swapEnabled);
            } else {
                _digits[i].Heal(digit);
            }
        }

        ApplySeparator(frame.SeparatorVisible);

        if (prev is { } pe && Settings.IsOClockEmp && ClockFrame.IsOClockMoment(pe,frame)) {
            TryEmphasize();
        }
    }

    void ApplySeparator(bool visible) {
        if (visible == _separatorVisible) return;
        _separatorVisible = visible;
        _separatorAnimator.Update(!visible);
    }

    void TryEmphasize() {
        long now = Environment.TickCount64;
        if (now < _emphasizeUntil) return;
        _emphasizeUntil = now + EmphasizeDurationMs;
        _emphasizeAnimator.Update();
    }

    void ApplyLayout() {
        bool isAccurate = Settings.IsAccurate;
        SecsPanel.IsVisible = isAccurate;
        SSecs.IsVisible = isAccurate;

        double gap = Math.Round(Settings.HorizontalGap);
        HoursPanel.Margin = new Thickness(0,0,gap,0);
        MinsPanel.Margin = new Thickness(gap,0,isAccurate ? gap : 0,0);
        SecsPanel.Margin = new Thickness(gap,0,0,0);
    }

    /// <summary>
    /// 同步强调背景的宽度为当前RootPanel宽度 (取整以避免亚像素模糊)
    /// </summary>
    void SyncBackgroundWidth() {
        double width = RootPanel.Bounds.Width;
        if (width > 0) {
            EmpBack.Width = Math.Round(width);
        }
    }

    #endregion

    /// <summary>
    /// 单个数字位: 封装 Label 与其翻页动画
    /// </summary>
    sealed class DigitSlot(ContentControl label) {
        static readonly string[] Glyphs = ["0","1","2","3","4","5","6","7","8","9"];

        readonly Animators.GenericContentSwapAnimator _animator = new Animators.GenericContentSwapAnimator(label);

        static string Glyph(char digit) => Glyphs[digit - '0'];

        /// <summary>
        /// 以动画切换到新数字
        /// </summary>
        public void Set(char digit,bool swap) {
            _animator.Update(Glyph(digit),true,swap);
        }

        /// <summary>
        /// 若当前未在动画中且显示内容与期望不符, 则静默修正
        /// </summary>
        public void Heal(char digit) {
            if (_animator.IsRendering) return;
            string glyph = Glyph(digit);
            if (label.Content as string == glyph) return;
            _animator.SilentUpdate(glyph);
        }
    }
}

/// <summary>
/// 流畅时钟某一时刻的期望显示状态 (纯数据, 不依赖 UI)
/// </summary>
/// <param name="Hour">时</param>
/// <param name="Minute">分</param>
/// <param name="Second">秒</param>
/// <param name="ShowSeconds">是否显示秒 (精确到秒)</param>
public readonly record struct ClockFrame(int Hour, int Minute, int Second, bool ShowSeconds) {
    /// <summary>
    /// 数字位数: HH MM SS
    /// </summary>
    public const int DigitCount = 6;

    public static ClockFrame From(DateTime time, bool showSeconds) =>
        new ClockFrame(time.Hour, time.Minute, time.Second, showSeconds);

    /// <summary>
    /// 分隔符是否可见: 精确到秒时常亮, 否则按秒奇偶闪烁 (与真实秒数对齐)
    /// </summary>
    public bool SeparatorVisible => ShowSeconds || Second % 2 == 0;

    /// <summary>
    /// 取第 <paramref name="index"/> 位数字 (0..5 分别为 时十/时个/分十/分个/秒十/秒个)
    /// </summary>
    public char DigitAt(int index) => index switch {
        0 => (char)('0' + Hour / 10),
        1 => (char)('0' + Hour % 10),
        2 => (char)('0' + Minute / 10),
        3 => (char)('0' + Minute % 10),
        4 => (char)('0' + Second / 10),
        5 => (char)('0' + Second % 10),
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    /// <summary>
    /// 该位是否属于秒
    /// </summary>
    public static bool IsSecondDigit(int index) => index >= 4;

    /// <summary>
    /// 是否为整点时刻: 小时发生变化且处于新小时的第一分钟内.
    /// 不要求恰好为第 0 秒, 以免 UI 卡顿跳帧时丢失强调; 同时排除任意时间跳变
    /// </summary>
    public static bool IsOClockMoment(ClockFrame prev, ClockFrame next) =>
        prev.Hour != next.Hour && next.Minute == 0;

    /// <summary>
    /// 秒数是否使用翻页动画.
    /// 专注模式下秒数改为淡入淡出, 但时/分翻页时同步翻页以保持整体一致
    /// </summary>
    public static bool ShouldSwapSeconds(ClockFrame prev, ClockFrame next, bool swapEnabled, bool focusedMode) =>
        swapEnabled && (!focusedMode || prev.Hour != next.Hour || prev.Minute != next.Minute);
}
