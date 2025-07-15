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
}