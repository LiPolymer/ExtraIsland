using System.Windows;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

[ComponentInfo(
    "759FFA33-309F-6494-3903-E0693036197E",
    "更好的倒计日",
    "\uE352",
    "提供更高级的功能与动画支持"
)]
public partial class BetterCountdown : ComponentBase<BetterCountdownConfig> {
    public BetterCountdown(ILessonsService lessonsService, IExactTimeService exactTimeService) {
        ExactTimeService = exactTimeService;
        LessonsService = lessonsService;
        InitializeComponent();
        _dyAnimator = new Animators.GenericContentSwapAnimator(LDays);
        _hrAnimator = new Animators.GenericContentSwapAnimator(LHours);
        _mnAnimator = new Animators.GenericContentSwapAnimator(LMins);
        _scAnimator = new Animators.GenericContentSwapAnimator(LSecs);
    }
    
    IExactTimeService ExactTimeService { get; }
    ILessonsService LessonsService { get; }

    readonly Animators.GenericContentSwapAnimator _dyAnimator;
    readonly Animators.GenericContentSwapAnimator _hrAnimator;
    readonly Animators.GenericContentSwapAnimator _mnAnimator;
    readonly Animators.GenericContentSwapAnimator _scAnimator;
    
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
        LSecs.IsVisible = (int)Settings.Accuracy >= 3;
        SSec.IsVisible = (int)Settings.Accuracy >= 3;
        LMins.IsVisible = (int)Settings.Accuracy >= 2;
        SMins.IsVisible = (int)Settings.Accuracy >= 2;
        LHours.IsVisible = (int)Settings.Accuracy >= 1;
        SHours.IsVisible = (int)Settings.Accuracy >= 1;
    }

    readonly Thickness _noGapThick = new Thickness(0);
    readonly Thickness _gapThick = new Thickness(2);
    void UpdateGap() {
        LSecs.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        SSec.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        LMins.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        SMins.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        LHours.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        SHours.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        LDays.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        Lp.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
        Ls.Padding = Settings.IsNoGapDisplay ? _noGapThick : _gapThick;
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
        TimeSpan span = EiUtils.GetDateTimeSpan(Now,Settings.TargetDateTime);
        if (_days != span.Days.ToString() | _isAccurateChanged) {
            int dayI = span.Days;
            int dayCi = (int)Settings.Accuracy == 0 & Settings.IsCorrectorEnabled ? dayI + 1 : dayI;
            _days = dayCi.ToString();
            bool isPassed = false;
            if ((int)Settings.Accuracy > 0 & Settings.IsHideZeroEnabled) {
                bool flag = _days == "0";
                LDays.IsVisible = !flag;
                SDays.IsVisible = !flag;
                if (flag) {
                    isPassed = true;
                }
            }
            if (!isPassed) {
                LDays.IsVisible = true;
                SDays.IsVisible = true;
                _dyAnimator.Update(_days, Settings.IsAnimationEnabled);                
            }
        }
        if ((_hours != span.Hours.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 1) {
            int hourI = span.Hours;
            int hourCi = (int)Settings.Accuracy == 1 & Settings.IsCorrectorEnabled ? hourI + 1 : hourI;
            _hours = hourCi.ToString();
            bool isPassed = false;
            if ((int)Settings.Accuracy > 1 & Settings.IsHideZeroEnabled) {
                bool flag = _hours == "0";
                LHours.IsVisible = !flag;
                SHours.IsVisible = !flag;
                if (flag) isPassed = true;
            }
            if (!isPassed & (int)Settings.Accuracy >= 1) {
                LHours.IsVisible = true;
                SHours.IsVisible = true;
                _hrAnimator.Update(_hours, Settings.IsAnimationEnabled);
            }
        }
        if ((_minutes != span.Minutes.ToString() | _isAccurateChanged) & (int)Settings.Accuracy >= 2) {
            int minuteI = span.Minutes;
            int minuteCi = (int)Settings.Accuracy == 2 & Settings.IsCorrectorEnabled ? minuteI + 1 : minuteI;
            _minutes = minuteCi.ToString();
            bool isPassed = false;
            if ((int)Settings.Accuracy > 2 & Settings.IsHideZeroEnabled) {
                bool flag = _minutes == "0";
                //LMins.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
                //SMins.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
                LMins.IsVisible = !flag;
                SMins.IsVisible = !flag;
                if (flag) isPassed = true;
            }
            if (!isPassed & (int)Settings.Accuracy >= 2) {
                LMins.IsVisible = true;
                SMins.IsVisible = true;
                string m = _minutes;
                if (m.Length == 1) {
                    m = "0" + m;
                }
                _mnAnimator.Update(m, Settings.IsAnimationEnabled);   
            }
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
        TimeSpan span = EiUtils.GetDateTimeSpan(Now,Settings.TargetDateTime);
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
    
    void BetterCountdown_OnLoaded(object? sender, RoutedEventArgs e) {
        Dispatcher.UIThread.InvokeAsync(OnLoad);
    }
    void BetterCountdown_OnUnloaded(object? sender,RoutedEventArgs e) {
        OnTimeChanged -= DetectEvent;
        Settings.OnAccuracyChanged -= UpdateAccuracy;
        Settings.OnNoGapDisplayChanged -= UpdateGap;
        LessonsService.PostMainTimerTicked -= UpdateTime;
    }
}