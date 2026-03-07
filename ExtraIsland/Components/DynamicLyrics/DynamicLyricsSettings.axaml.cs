using ClassIsland.Core.Abstractions.Controls;

namespace ExtraIsland.Components;

public partial class DynamicLyricsSettings : ComponentBase<DynamicLyricsConfig> {
    public DynamicLyricsSettings() {
        InitializeComponent();
    }

    public List<LyricsDisplayType> DisplayTypes { get; } = [
        LyricsDisplayType.MainLine,
        LyricsDisplayType.SubLine
    ];
}