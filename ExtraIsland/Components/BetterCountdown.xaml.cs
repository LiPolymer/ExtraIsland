using System.Windows;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
    "759FFA33-309F-6494-3903-E0693036197E",
    "更好的倒计日",
    PackIconKind.CalendarOutline,
    "提供更高级的功能与动画支持"
)]
public partial class BetterCountdown {
    public BetterCountdown(ILessonsService lessonsService, IExactTimeService exactTimeService) {
        ExactTimeService = exactTimeService;
        LessonsService = lessonsService;
        InitializeComponent();
        _dyAnimator = new Animators.ClockTransformControlAnimator(LDays);
        _hrAnimator = new Animators.ClockTransformControlAnimator(LHours);
        _mnAnimator = new Animators.ClockTransformControlAnimator(LMins);
        _scAnimator = new Animators.ClockTransformControlAnimator(LSecs);
    }
    
    IExactTimeService ExactTimeService { get; }
    ILessonsService LessonsService { get; }

    readonly Animators.ClockTransformControlAnimator _dyAnimator;
    readonly Animators.ClockTransformControlAnimator _hrAnimator;
    readonly Animators.ClockTransformControlAnimator _mnAnimator;
    readonly Animators.ClockTransformControlAnimator _scAnimator;
    
    void OnLoad() {
        UpdateTime();
        UpdateAccuracy();
        UpdateGap();
        SilentUpdater();
        OnTimeChanged += DetectEvent;
        Settings.OnAccuracyChanged += UpdateAccuracy;
        Settings.OnNoGapDisplayChanged += UpdateGap;
        LessonsService.PostMainTimerTicked += UpdateTime;
    }

    bool _isAccurateChanged;
    void UpdateAccuracy() {
        _isAccurateChanged = true;
        LSecs.Visibility = BoolToCollapsedVisible((int)Settings.Accuracy >= 3);
        SSec.Visibility = BoolToCollapsedVisible((int)Settings.Accuracy >= 3);
        LMins.Visibility = BoolToCollapsedVisible((int)Settings.Accuracy >= 2);
        SMin.Visibility = BoolToCollapsedVisible((int)Settings.Accuracy >= 2);
        LHours.Visibility = BoolToCollapsedVisible((int)Settings.Accuracy >= 1);
        SHour.Visibility = BoolToCollapsedVisible((int)Settings.Accuracy >= 1);
    }

    readonly Thickness _noGapThick = new Thickness(0);
    readonly Thickness _gapThick = new Thickness(2);
    void UpdateGap() {
        LSecs.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        SSec.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        LMins.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        SMin.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        LHours.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        SHour.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        LDays.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        Lp.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        Ls.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
    }

    static Visibility BoolToCollapsedVisible(bool isVisible) {
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
    
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
    
    void UpdateTime(object? sender = null,EventArgs? eventArgs = null) {
        Now = !Settings.IsSystemTime ? 
            ExactTimeService.GetCurrentLocalDateTime()
            : DateTime.Now;
    }
    
    string _days = string.Empty;
    string _hours = string.Empty;
    string _minutes = string.Empty;
    string _seconds = string.Empty;
    bool _updateLock;
    void DetectEvent() {
        if (_updateLock) return;
        _updateLock = true;
        TimeSpan span = EiUtils.GetDateTimeSpan(Now,Settings.TargetDate);
        if (_days != span.Days.ToString() | _isAccurateChanged) {
            int dayI = span.Days;
            int dayCi = (int)Settings.Accuracy == 0 & Settings.IsCorrectorEnabled ? dayI + 1 : dayI;
            _days = dayCi.ToString();
            _dyAnimator.Update(_days, Settings.IsAnimationEnabled);
        }
        if ((_hours != span.Hours.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 1) {
            int hourI = span.Hours;
            int hourCi = (int)Settings.Accuracy == 1 & Settings.IsCorrectorEnabled ? hourI + 1 : hourI;
            _hours = hourCi.ToString();
            _hrAnimator.Update(_hours, Settings.IsAnimationEnabled);
        }
        if ((_minutes != span.Minutes.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 2) {
            int minuteI = span.Minutes;
            int minuteCi = (int)Settings.Accuracy == 2 & Settings.IsCorrectorEnabled ? minuteI + 1 : minuteI;
            _minutes = minuteCi.ToString();
            string m = _minutes;
            if (m.Length == 1) {
                m = "0" + m;
            }
            _mnAnimator.Update(m, Settings.IsAnimationEnabled);
        }
        // ReSharper disable once InvertIf
        if ((_seconds != span.Seconds.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 3) {
            _seconds = span.Seconds.ToString();
            string s = _seconds;
            if (s.Length == 1) {
                s = "0" + s;
            }
            _scAnimator.Update(s, Settings.IsAnimationEnabled, !Settings.IsFocusedModeEnabled);
            _isAccurateChanged = false;
        }
        _updateLock = false;
    }

    void SilentUpdater() {
        TimeSpan span = EiUtils.GetDateTimeSpan(Now,Settings.TargetDate);
        if (_days != span.Days.ToString() | _isAccurateChanged) {
            int dayI = span.Days;
            _days = (int)Settings.Accuracy == 0 ? (dayI + 1).ToString() : dayI.ToString();
            _dyAnimator.SilentUpdate(_days);
        }
        if ((_hours != span.Hours.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 1) {
            int hourI = span.Hours;
            _hours = (int)Settings.Accuracy == 1 ? (hourI + 1).ToString() : hourI.ToString();
            _hrAnimator.SilentUpdate(_hours);
        }
        if ((_minutes != span.Minutes.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 2) {
            int minuteI = span.Minutes;
            _minutes = (int)Settings.Accuracy == 2 ? (minuteI + 1).ToString() : minuteI.ToString();
            string m = _minutes;
            if (m.Length == 1) {
                m = "0" + m;
            }
            _mnAnimator.SilentUpdate(m);
        }
        // ReSharper disable once InvertIf
        if ((_seconds != span.Seconds.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 3) {
            _seconds = span.Seconds.ToString();
            string s = _seconds;
            if (s.Length == 1) {
                s = "0" + s;
            }
            _scAnimator.SilentUpdate(s);
        }
    }
    
    void BetterCountdown_OnLoaded(object sender, RoutedEventArgs e) {
        this.BeginInvoke(OnLoad);
    }
    void BetterCountdown_OnUnloaded(object sender,RoutedEventArgs e) {
        OnTimeChanged -= DetectEvent;
        Settings.OnAccuracyChanged -= UpdateAccuracy;
        Settings.OnNoGapDisplayChanged -= UpdateGap;
        LessonsService.PostMainTimerTicked -= UpdateTime;
    }
}