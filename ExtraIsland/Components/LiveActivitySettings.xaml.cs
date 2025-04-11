using System.Windows;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class LiveActivitySettings {
    public LiveActivitySettings() {
        IsLyricsIslandLoaded = EiUtils.IsLyricsIslandInstalled();
        IsLifeModeEnabled = GlobalConstants.Handlers.MainConfig!.Data.IsLifeModeActivated;
        InitializeComponent();
        // TODO: 修改为绑定+转换器
        ConflictHintContainer.Visibility = IsLyricsIslandLoaded ? Visibility.Visible : Visibility.Collapsed;
    }

    public bool IsLifeModeEnabled { get; }
    bool IsLyricsIslandLoaded { get; }
    public bool IsLyricsIslandNotLoaded { get => !IsLyricsIslandLoaded; }
}