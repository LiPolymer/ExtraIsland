using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
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
        AccurateModeUpdater();
        UpdateTime();
        SilentUpdater();
        UpdateGaps();
        if (Settings.IsSecondsSmall) {
            SmallSecondsUpdater();
        }
        //Register Events
        Settings.OnSecondsSmallChanged += SmallSecondsUpdater;
        Settings.OnAccurateChanged += AccurateModeUpdater;
        Settings.OnOClockEmpEnabled += ShowEmphasise;
        Settings.OnLayoutGapChanged += UpdateGaps;
        LessonsService.PostMainTimerTicked += UpdateTime;
        OnTimeChanged += () => {
            if (updLock) return;
            updLock = true;
            MainUpdater();
            Dispatcher.UIThread.Post(SyncBackgroundWidth);
        };
        return;

        void MainUpdater() {
            DateTime handlingTime = Now;
            if (hours != Now.Hour.ToString()) {
                if (Settings.IsOClockEmp && Now.Second == 0) {
                    _emphasizeAnimator.Update();
                }
                hours = Now.Hour.ToString("D2");
                _hourAnimator.Update(hours, true, Settings.IsSwapAnimationEnabled);
            }
            if (minutes != Now.Minute.ToString()) {
                minutes = Now.Minute.ToString("D2");
                _minuAnimator.Update(minutes, true, Settings.IsSwapAnimationEnabled);
            }
            if (seconds != Now.Second.ToString()) {
                seconds = Now.Second.ToString("D2");
                if (Settings.IsAccurate) {
                    SMins.Opacity = 1;
                    _secoAnimator.Update(seconds, true, !(Settings.IsFocusedMode || !Settings.IsSwapAnimationEnabled));
                }
            }
            // Unlocker
            if (handlingTime == Now) {
                updLock = false;
            } else {
                MainUpdater();
            }
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
        try {
            EmpBack.Width = Math.Round(RootPanel.Bounds.Width);
        }
        catch {
            // ignored
        }
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

    void SmallSecondsUpdater() {
        Dispatcher.UIThread.InvokeAsync(() => {
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
            bool isSmall = Settings.IsSecondsSmall;
            /*LSecs.Bind(FontSizeProperty,
                       new DynamicResourceExtension(isSmall 
                                                        ? "MainWindowSecondaryFontSize" 
                                                        : "MainWindowEmphasizedFontSize")
                           .ProvideValue(null!));*/
            //Console.WriteLine("Hola!");
            SyncBackgroundWidth();
        });
    }

    void AccurateModeUpdater() {
        Dispatcher.UIThread.InvokeAsync(() => {
            SMins.Opacity = 1;
            LSecs.IsVisible = Settings.IsAccurate;
            SSecs.IsVisible = Settings.IsAccurate;
            Placeholder1.Content = Settings.IsAccurate ? "00:00:00" : "00:00";
            Placeholder2.Content = Settings.IsAccurate ? "00:00:00" : "00:00";
            SyncBackgroundWidth();
            EnsureSeparatorBlinkingState();
        });
    }

    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        Dispatcher.UIThread.InvokeAsync(LoadedAction);
    }
    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        Settings.OnAccurateChanged -= AccurateModeUpdater;
        Settings.OnSecondsSmallChanged -= SmallSecondsUpdater;
        Settings.OnOClockEmpEnabled -= ShowEmphasise;
        Settings.OnLayoutGapChanged -= UpdateGaps;
        LessonsService.PostMainTimerTicked -= UpdateTime;
        StopSeparatorBlinking();
    }

    void EnsureSeparatorBlinkingState() {
        try {
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
        } catch {
            // ignored
        }
    }

    void StopSeparatorBlinking() {
        try {
            if (_separatorBlinkTimer != null) {
                _separatorBlinkTimer.Stop();
                _separatorBlinkTimer.Tick -= SeparatorBlinkTick;
                _separatorBlinkTimer = null;
            }
        } catch {
            // ignored
        }
    }

    void SeparatorBlinkTick(object? sender, EventArgs e) {
        try {
            _separatorBlinkInvisible = !_separatorBlinkInvisible;
            _separatorAnimator.Update(_separatorBlinkInvisible);
        } catch {
            // ignored
        }
    }

    void UpdateGaps() {
        Dispatcher.UIThread.InvokeAsync(() => {
            double gap = Math.Round(Settings.HorizontalGap);
            try
            {
                LHours.Padding = new Thickness(0,0,gap,0);
                LMins.Padding = new Thickness(gap,0,gap,0);
                LSecs.Padding = new Thickness(gap,0,0,0);
                SyncBackgroundWidth();
            }
            catch
            {
                // ignored
            }
        });
    }
}
