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
        if (Settings.IsSecondsSmall) {
            SmallSecondsUpdater();
        }
        //Register Events
        Settings.OnSecondsSmallChanged += SmallSecondsUpdater;
        Settings.OnAccurateChanged += AccurateModeUpdater;
        Settings.OnOClockEmpEnabled += ShowEmphasise;
        LessonsService.PostMainTimerTicked += UpdateTime;
        OnTimeChanged += () => {
            if (updLock) return;
            updLock = true;
            MainUpdater();
        };
        return;
        
        void MainUpdater() {
            DateTime handlingTime = Now;
            if (hours != Now.Hour.ToString()) {
                if (Settings.IsOClockEmp & Now.Second == 0) {
                    _emphasizeAnimator.Update();
                }
                hours = Now.Hour.ToString();
                string h = hours;
                _hourAnimator.Update(h,true,Settings.IsSwapAnimationEnabled);
            }
            if (minutes != Now.Minute.ToString()) {
                minutes = Now.Minute.ToString();
                string m = minutes;
                if (m.Length == 1) {
                    m = "0" + m;
                }
                _minuAnimator.Update(m,true,Settings.IsSwapAnimationEnabled);
            }
            if (seconds != Now.Second.ToString()) {
                seconds = Now.Second.ToString();
                if (Settings.IsAccurate) {
                    SMins.Opacity = 1;
                    string s = seconds;
                    if (s.Length == 1) {
                        s = "0" + s;
                    }
                    _secoAnimator.Update(s,true,!(Settings.IsFocusedMode | !Settings.IsSwapAnimationEnabled));
                } else {
                    bool seq = sparkSeq;
                    _separatorAnimator.Update(seq);
                    sparkSeq = !sparkSeq;
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
            hours = Now.Hour.ToString();
            minutes = Now.Minute.ToString();
            if (minutes.Length == 1) {
                minutes = "0" + minutes;
            }
            seconds = Now.Second.ToString();
            if (seconds.Length == 1) {
                seconds = "0" + seconds;
            }
            _hourAnimator.SilentUpdate(hours);
            _minuAnimator.SilentUpdate(minutes);
            _secoAnimator.SilentUpdate(seconds);
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
            LSecs.Bind(FontSizeProperty,
                       new DynamicResourceExtension(isSmall ? "MainWindowSecondaryFontSize" : "MainWindowEmphasizedFontSize")
                           .ProvideValue(null!));
        });
    }

    void AccurateModeUpdater() {
        Dispatcher.UIThread.InvokeAsync(() => {
            SMins.Opacity = 1;
            //LSecs.Visibility = Settings.IsAccurate ? Visibility.Visible : Visibility.Collapsed;
            //SSecs.Visibility = Settings.IsAccurate ? Visibility.Visible : Visibility.Collapsed;
            LSecs.IsVisible = Settings.IsAccurate;
            SSecs.IsVisible = Settings.IsAccurate;
            Placeholder1.Content = Settings.IsAccurate ? "00:00:00" : "00:00";
            Placeholder2.Content = Settings.IsAccurate ? "00:00:00" : "00:00";
        });
    }
    
    void OnLoaded(object? sender,RoutedEventArgs e) {
        Dispatcher.UIThread.InvokeAsync(LoadedAction);
    }
    void OnUnloaded(object? sender,RoutedEventArgs e) {
        Settings.OnAccurateChanged -= AccurateModeUpdater;
        Settings.OnSecondsSmallChanged -= SmallSecondsUpdater;
        Settings.OnOClockEmpEnabled -= ShowEmphasise;
        LessonsService.PostMainTimerTicked -= UpdateTime;
    }
}