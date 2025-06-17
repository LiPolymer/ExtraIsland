namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class FluentClockConfig {

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

    public bool IsSwapAnimationEnabled { get; set; } = true;

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