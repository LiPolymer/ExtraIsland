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
    DispatcherTimer? _separatorBlinkTimer;
    bool _separatorBlinkInvisible;
    bool _pendingWidthSync;
    CancellationTokenSource? _cts;
    public FluentClock(ILessonsService lessonsService,IExactTimeService exactTimeService) {
        ExactTimeService = exactTimeService;
        LessonsService = lessonsService;
        InitializeComponent();
        _hourTensAnimator = new Animators.GenericContentSwapAnimator(LHourTens);
        _hourUnitsAnimator = new Animators.GenericContentSwapAnimator(LHourUnits);
        _minTensAnimator = new Animators.GenericContentSwapAnimator(LMinTens);
        _minUnitsAnimator = new Animators.GenericContentSwapAnimator(LMinUnits);
        _secTensAnimator = new Animators.GenericContentSwapAnimator(LSecTens);
        _secUnitsAnimator = new Animators.GenericContentSwapAnimator(LSecUnits);
        _separatorAnimator = new Animators.SeparatorVisualAnimator(SMins);
        _emphasizeAnimator = new Animators.EmphasizerVisualAnimator(EmpBack);
    }

    IExactTimeService ExactTimeService { get; }
    ILessonsService LessonsService { get; }

    DateTime _nowTime;
    DateTime Now {
        get => _nowTime;
        set {
            if (_nowTime == value) return;
            _nowTime = value;
            OnTimeChanged?.Invoke();
        }
    }
    event Action? OnTimeChanged;

    readonly Animators.GenericContentSwapAnimator _hourTensAnimator;
    readonly Animators.GenericContentSwapAnimator _hourUnitsAnimator;
    readonly Animators.GenericContentSwapAnimator _minTensAnimator;
    readonly Animators.GenericContentSwapAnimator _minUnitsAnimator;
    readonly Animators.GenericContentSwapAnimator _secTensAnimator;
    readonly Animators.GenericContentSwapAnimator _secUnitsAnimator;
    readonly Animators.SeparatorVisualAnimator _separatorAnimator;
    readonly Animators.EmphasizerVisualAnimator _emphasizeAnimator;

    void LoadedAction() {
        //Prepare local variable

        string hours = "--";
        string minutes = "--";
        string seconds = "--";

        bool updLock = false;
        //Initialization
        UpdateSecondsAppearance();
        UpdateTime();
        SilentUpdater();
        UpdateGaps();
        //Register Events
        Settings.OnSecondsSmallChanged += UpdateSecondsAppearance;
        Settings.OnAccurateChanged += UpdateSecondsAppearance;
        Settings.OnOClockEmpEnabled += ShowEmphasise;
        Settings.OnLayoutGapChanged += UpdateGaps;
        LessonsService.PostMainTimerTicked += UpdateTime;
        OnTimeChanged += () => {
            if (updLock) return;
            updLock = true;
            MainUpdater();
            RequestSyncBackgroundWidth();
        };
        return;

        void MainUpdater() {
            DateTime handlingTime = Now;
            while (true) {
                int h = handlingTime.Hour;
                int m = handlingTime.Minute;
                int s = handlingTime.Second;
                string hoursStr = h.ToString("D2");
                string minsStr = m.ToString("D2");
                string secsStr = s.ToString("D2");

                bool swapEnabled = Settings.IsSwapAnimationEnabled;

                if (hours != hoursStr) {
                    if (Settings.IsOClockEmp && s == 0) {
                        _emphasizeAnimator.Update();
                    }
                    hours = hoursStr;
                    _hourTensAnimator.Update(hours[0].ToString(), true, swapEnabled);
                    _hourUnitsAnimator.Update(hours[1].ToString(), true, swapEnabled);
                }
                
                bool minutesChanged = minutes != minsStr;
                if (minutesChanged) {
                    minutes = minsStr;
                    _minTensAnimator.Update(minutes[0].ToString(), true, swapEnabled);
                    _minUnitsAnimator.Update(minutes[1].ToString(), true, swapEnabled);
                }
                
                if (seconds != secsStr) {
                    seconds = secsStr;
                    if (Settings.IsAccurate) {
                        SMins.Opacity = 1;
                        // Focus Mode Sync: Force animation if minutes just flipped
                        bool forceSync = Settings.IsFocusedMode && minutesChanged;
                        bool shouldSwap = swapEnabled && (!Settings.IsFocusedMode || forceSync);
                        
                        _secTensAnimator.Update(seconds[0].ToString(), true, shouldSwap);
                        _secUnitsAnimator.Update(seconds[1].ToString(), true, shouldSwap);
                    }
                }
                if (handlingTime == Now) break;
                handlingTime = Now;
            }
            updLock = false;
        }

        void SilentUpdater() {
            hours = Now.Hour.ToString("D2");
            minutes = Now.Minute.ToString("D2");
            seconds = Now.Second.ToString("D2");
            _hourTensAnimator.SilentUpdate(hours[0].ToString());
            _hourUnitsAnimator.SilentUpdate(hours[1].ToString());
            _minTensAnimator.SilentUpdate(minutes[0].ToString());
            _minUnitsAnimator.SilentUpdate(minutes[1].ToString());
            _secTensAnimator.SilentUpdate(seconds[0].ToString());
            _secUnitsAnimator.SilentUpdate(seconds[1].ToString());
        }
    }

    /// <summary>
    /// 同步强调背景的宽度为当前RootPanel宽度
    /// </summary>
    void SyncBackgroundWidth() {
        double width = RootPanel?.Bounds.Width ?? 0;
        if (width > 0) {
            EmpBack.Width = Math.Round(width);
        }
    }

    void RequestSyncBackgroundWidth() {
        if (_pendingWidthSync) return;
        _pendingWidthSync = true;
        Dispatcher.UIThread.Post(() => {
            if (_cts?.IsCancellationRequested ?? false) {
                _pendingWidthSync = false;
                return;
            }
            _pendingWidthSync = false;
            SyncBackgroundWidth();
        },DispatcherPriority.Background);
    }

    void ShowEmphasise() {
        _emphasizeAnimator.Update();
    }

    void UpdateTime(object? sender,EventArgs e) {
        UpdateTime();
    }

    void UpdateTime() {
        Now = !Settings.IsSystemTime ?
            ExactTimeService.GetCurrentLocalDateTime()
            : DateTime.Now;
        Reconcile();
    }
    
    void Reconcile() {
        Dispatcher.UIThread.InvokeAsync(() => {
            if (_cts?.IsCancellationRequested ?? false) return;
            string hours = Now.Hour.ToString("D2");
            string minutes = Now.Minute.ToString("D2");
            string seconds = Now.Second.ToString("D2");
            SyncDigit(LHourTens,hours[0],_hourTensAnimator);
            SyncDigit(LHourUnits,hours[1],_hourUnitsAnimator);
            SyncDigit(LMinTens,minutes[0],_minTensAnimator);
            SyncDigit(LMinUnits,minutes[1],_minUnitsAnimator);
            SyncDigit(LSecTens,seconds[0],_secTensAnimator);
            SyncDigit(LSecUnits,seconds[1],_secUnitsAnimator);
        });
    }

    void SyncDigit(ContentControl label,char expected,Animators.GenericContentSwapAnimator animator) {
        if (animator.IsRendering) return;
        string expectedStr = expected.ToString();
        if (label.Content as string == expectedStr) return;
        animator.SilentUpdate(expectedStr);
    }

    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        Dispatcher.UIThread.InvokeAsync(() => {
            if (_cts?.IsCancellationRequested ?? false) return;
            LoadedAction();
        });
    }
    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        Settings.OnAccurateChanged -= UpdateSecondsAppearance;
        Settings.OnSecondsSmallChanged -= UpdateSecondsAppearance;
        Settings.OnOClockEmpEnabled -= ShowEmphasise;
        Settings.OnLayoutGapChanged -= UpdateGaps;
        LessonsService.PostMainTimerTicked -= UpdateTime;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _pendingWidthSync = false;
        StopSeparatorBlinking();
    }

    void EnsureSeparatorBlinkingState() {
        if (!Settings.IsAccurate) {
            if (_separatorBlinkTimer == null) {
                _separatorBlinkInvisible = false; // 初始为可见
                _separatorBlinkTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                _separatorBlinkTimer.Tick += SeparatorBlinkTick;
                _separatorBlinkTimer.Start();
            }
        } else {
            _separatorAnimator.Update(false);
            StopSeparatorBlinking();
        }
    }

    void StopSeparatorBlinking() {
        if (_separatorBlinkTimer != null) {
            _separatorBlinkTimer.Stop();
            _separatorBlinkTimer.Tick -= SeparatorBlinkTick;
            _separatorBlinkTimer = null;
        }
    }

    void SeparatorBlinkTick(object? sender,EventArgs e) {
        _separatorBlinkInvisible = !_separatorBlinkInvisible;
        _separatorAnimator.Update(_separatorBlinkInvisible);
    }

    void UpdateGaps() {
        Dispatcher.UIThread.InvokeAsync(() => {
            if (_cts?.IsCancellationRequested ?? false) return;
            double gap = Math.Round(Settings.HorizontalGap);
            HoursPanel.Margin = new Thickness(0,0,gap,0);
            MinsPanel.Margin = new Thickness(gap,0,gap,0);
            SecsPanel.Margin = new Thickness(gap,0,0,0);
            RequestSyncBackgroundWidth();
        });
    }

    void UpdateSecondsAppearance() {
        Dispatcher.UIThread.InvokeAsync(() => {
            if (_cts?.IsCancellationRequested ?? false) return;
            bool isAccurate = Settings.IsAccurate;
            bool isSmall = Settings.IsSecondsSmall;
            SecsPanel.IsVisible = isAccurate;
            SSecs.IsVisible = isAccurate;
            Placeholder1.Content = isAccurate ? "00:00:00" : "00:00";
            Placeholder2.Content = isAccurate ? "00:00:00" : "00:00";
            if (isAccurate) {
                SMins.Opacity = 1;
            }
            RequestSyncBackgroundWidth();
            EnsureSeparatorBlinkingState();
        });
    }
}
