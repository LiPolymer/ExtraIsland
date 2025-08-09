using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class LiveActivitySettings : ComponentBase<LiveActivityConfig> {
    public LiveActivitySettings() {
        IsLyricsIslandLoaded = EiUtils.IsLyricsIslandInstalled();
        IsLifeModeEnabled = GlobalConstants.Handlers.MainConfig!.Data.IsLifeModeActivated;
        InitializeComponent();
    }

    void DeleteButton_Click(object sender,RoutedEventArgs e) {
        Button button = (sender as Button)!;
        if (button.DataContext is ReplaceItem item) {
            Settings.ReplacementsList.Remove(item);
        }
    }
    
    void ButtonBase_OnClick(object sender,RoutedEventArgs e) {
        Settings.ReplacementsList.Add(new ReplaceItem());
    }
    
    public bool IsLifeModeEnabled { get; }
    bool IsLyricsIslandLoaded { get; }
    public bool IsLyricsIslandNotLoaded { get => !IsLyricsIslandLoaded; }
}