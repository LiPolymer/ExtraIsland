using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using System.Threading;
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
        _hourAnimator = new Animators.GenericContentSwapAnimator(LHours);
        _minuAnimator = new Animators.GenericContentSwapAnimator(LMins);
        _secoAnimator = new Animators.GenericContentSwapAnimator(LSecs);
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

    readonly Animators.GenericContentSwapAnimator _hourAnimator;
    readonly Animators.GenericContentSwapAnimator _minuAnimator;
    readonly Animators.GenericContentSwapAnimator _secoAnimator;
    readonly Animators.SeparatorVisualAnimator _separatorAnimator;
    readonly Animators.EmphasizerVisualAnimator _emphasizeAnimator;

    void LoadedAction() {
        //Prepare local variable

        string hours;
        string minutes;
        string seconds;

        bool sparkSeq = true;
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
                if (hours != hoursStr) {
                    if (Settings.IsOClockEmp && s == 0) {
                        _emphasizeAnimator.Update();
                    }
                    hours = hoursStr;
                    _hourAnimator.Update(hours,true,Settings.IsSwapAnimationEnabled);
                }
                if (minutes != minsStr) {
                    minutes = minsStr;
                    _minuAnimator.Update(minutes,true,Settings.IsSwapAnimationEnabled);
                }
                if (seconds != secsStr) {
                    seconds = secsStr;
                    if (Settings.IsAccurate) {
                        SMins.Opacity = 1;
                        _secoAnimator.Update(seconds,true,!(Settings.IsFocusedMode || !Settings.IsSwapAnimationEnabled));
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
            _hourAnimator.SilentUpdate(hours);
            _minuAnimator.SilentUpdate(minutes);
            _secoAnimator.SilentUpdate(seconds);
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
            LHours.Padding = new Thickness(0,0,gap,0);
            LMins.Padding = new Thickness(gap,0,gap,0);
            LSecs.Padding = new Thickness(gap,0,0,0);
            RequestSyncBackgroundWidth();
        });
    }

    void UpdateSecondsAppearance() {
        Dispatcher.UIThread.InvokeAsync(() => {
            if (_cts?.IsCancellationRequested ?? false) return;
            bool isAccurate = Settings.IsAccurate;
            bool isSmall = Settings.IsSecondsSmall;
            LSecs.IsVisible = isAccurate;
            SSecs.IsVisible = isAccurate;
            Placeholder1.Content = isAccurate ? "00:00:00" : "00:00";
            Placeholder2.Content = isAccurate ? "00:00:00" : "00:00";
            if (isAccurate) {
                SMins.Opacity = 1;
            }
            //todo: 恢复秒数小字号
            /*
            bool isSmall = Settings.IsSecondsSmall;

            LSecs.SetResourceReference(FontSizeProperty,
                isSmall ? "MainWindowSecondaryFontSize" : "MainWindowEmphasizedFontSize");

            LSecs.Padding = isSmall ?
                new Thickness(0,3,0,0)
                : new Thickness(0);

            SSecs.Padding = isSmall ?
                new Thickness(0,1,0,0)
                : new Thickness(0,0,0,3);

            SSecs.SetResourceReference(FontSizeProperty,
                isSmall ? "MainWindowSecondaryFontSize" : "MainWindowLargeFontSize");

            TSecs.X = isSmall ? 2 : 0; */
            /*LSecs.Bind(FontSizeProperty,
                       new DynamicResourceExtension(isSmall
                                                        ? "MainWindowSecondaryFontSize"
                                                        : "MainWindowEmphasizedFontSize")
                           .ProvideValue(null!));*/
            //Console.WriteLine("Hola!");
            RequestSyncBackgroundWidth();
            EnsureSeparatorBlinkingState();
        });
    }
}
