using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class FluentClockConfig : ObservableObject {

    bool _isAccurate = true;
    public bool IsAccurate {
        get => _isAccurate;
        set {
            if (_isAccurate == value) return;
            _isAccurate = value; 
            OnAccurateChanged?.Invoke();
        }
    }
    public event Action? OnAccurateChanged;

    public bool IsFocusedMode { get; set; }

    bool _isSwapAnimationEnabled = true;
    public bool IsSwapAnimationEnabled { 
        get => _isSwapAnimationEnabled;
        set {
            if (_isSwapAnimationEnabled == value) return;
            _isSwapAnimationEnabled = value;
            OnPropertyChanged();
        } 
    }

    bool _isSecondsSmall;
    public bool IsSecondsSmall {
        get => _isSecondsSmall;
        set {
            if (_isSecondsSmall == value) return;
            _isSecondsSmall = value;
            OnPropertyChanged();
            OnSecondsSmallChanged?.Invoke();
        }
    }
    public event Action? OnSecondsSmallChanged;

    public bool IsSystemTime { get; set; }
    
    bool _isOClockEmp = true;

    public bool IsOClockEmp {
        get => _isOClockEmp;
        set {
            _isOClockEmp = value;
            if (_isOClockEmp) {
                OnOClockEmpEnabled?.Invoke();
            }
        }
    }

    public event Action? OnOClockEmpEnabled;

    // Layout: horizontal gap between hour/colon/minute/colon/second (in px)
    double _horizontalGap = 2;
    public double HorizontalGap {
        get => _horizontalGap;
        set {
            double newVal = Math.Round(value);
            if (Math.Abs(_horizontalGap - newVal) < 0.0001) return;
            _horizontalGap = newVal;
            OnPropertyChanged();
            OnLayoutGapChanged?.Invoke();
        }
    }
    public event Action? OnLayoutGapChanged;
}
