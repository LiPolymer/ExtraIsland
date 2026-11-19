using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
// NOTE: 公开属性名即持久化键名, 请勿随意更改
public partial class FluentClockConfig : ObservableObject {
    [ObservableProperty]
    bool _isAccurate = true;

    [ObservableProperty]
    bool _isFocusedMode;

    [ObservableProperty]
    bool _isSwapAnimationEnabled = true;

    [ObservableProperty]
    bool _isSecondsSmall;

    [ObservableProperty]
    bool _isSystemTime;

    [ObservableProperty]
    bool _isOClockEmp = true;

    // Layout: horizontal gap between hour/colon/minute/colon/second (in px)
    double _horizontalGap = 2;
    public double HorizontalGap {
        get => _horizontalGap;
        set => SetProperty(ref _horizontalGap, Math.Round(value));
    }
}
