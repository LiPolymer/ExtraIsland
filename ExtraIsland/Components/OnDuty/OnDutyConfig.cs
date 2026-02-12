using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

public partial class OnDutyConfig : ObservableObject {
    [ObservableProperty]
    bool _isCompactModeEnabled = false;
}