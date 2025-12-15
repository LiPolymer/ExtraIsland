using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

public partial class DynamicLyricsConfig : ObservableObject {
    [ObservableProperty]
    LyricsDisplayType _displayType = LyricsDisplayType.MainLine;
}

public enum LyricsDisplayType {
    [Description("主行")]
    MainLine,
    [Description("副行")]
    SubLine
}