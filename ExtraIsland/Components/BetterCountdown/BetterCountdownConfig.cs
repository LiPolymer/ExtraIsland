using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class BetterCountdownConfig : ObservableObject {
    public BetterCountdownConfig() {
        Separators.PropertyChanged += (_,_) => OnPropertyChanged();
    }
    
    public DateTime TargetDateTime { get; set; } = DateTime.Now;
    
    string _prefix = "现在";
    public string Prefix {
        get => _prefix;
        set {
            if (value == _prefix) return;
            _prefix = value;
            OnPropertyChanged();
        }
    }
    
    string _suffix = "过去了";
    public string Suffix {
        get => _suffix;
        set {
            if (value == _suffix) return;
            _suffix = value;
            OnPropertyChanged();
        }
    }
    public bool IsSystemTime { get; set; }
    public bool IsCorrectorEnabled { get; set; } = true;
    bool _isHideZeroEnabled = false;
    public bool IsHideZeroEnabled {
        get=>_isHideZeroEnabled;
        set {
            if (_isHideZeroEnabled == value) return;
            _isHideZeroEnabled = value;
            OnAccuracyChanged?.Invoke();
        }
    }
    public CountdownSeparatorConfigs Separators { get; set; } = new CountdownSeparatorConfigs();

    bool _isNoGapDisplay;
    public bool IsNoGapDisplay {
        get => _isNoGapDisplay;
        set {
            if (_isNoGapDisplay == value) return;
            _isNoGapDisplay = value;
            OnNoGapDisplayChanged?.Invoke();
        }
    }
    public event Action? OnNoGapDisplayChanged;
    
    public bool IsAnimationEnabled { get; set; } = true;
    
    CountdownAccuracy _accuracy = CountdownAccuracy.Minute;
    public CountdownAccuracy Accuracy {
        get => _accuracy;
        set {
            if (_accuracy == value) return;
            _accuracy = value;
            OnAccuracyChanged?.Invoke();
        }
    }
    public event Action? OnAccuracyChanged;
    
    public bool IsFocusedModeEnabled { get; set; } = false;
}

public class CountdownSeparatorConfigs : ObservableObject {
    string _day = "天";
    string _hour = "时";
    string _minute = "分";
    string _second = "秒";
    public string Day {
        get => _day;
        set {
            if (value == _day) return;
            _day = value;
            OnPropertyChanged();
        }
    }
    public string Hour {
        get => _hour;
        set {
            if (value == _hour) return;
            _hour = value;
            OnPropertyChanged();
        }
    }
    public string Minute {
        get => _minute;
        set {
            if (value == _minute) return;
            _minute = value;
            OnPropertyChanged();
        }
    }
    public string Second {
        get => _second;
        set {
            if (value == _second) return;
            _second = value;
            OnPropertyChanged();
        }
    }
}

public enum CountdownAccuracy {
    [Description("天")]
    Day,
    [Description("时")]
    Hour,
    [Description("分")]
    Minute,
    [Description("秒")]
    Second
}