using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class FlagDisplayConfig : ObservableObject {
    public string TargetFlag { get; set; } = string.Empty;

    public string FallbackText { get; set; } = string.Empty;

    public bool IsAnimationEnabled { get; set; } = true;

    public bool IsSwapAnimationEnabled { get; set; }
}
