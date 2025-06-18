using System.Windows;
using System.Windows.Controls;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;
using YamlDotNet.Serialization;

namespace ExtraIsland.Components;

public partial class LiveActivitySettings {
    public LiveActivitySettings() {
        IsLyricsIslandLoaded = EiUtils.IsLyricsIslandInstalled();
        IsLifeModeEnabled = GlobalConstants.Handlers.MainConfig!.Data.IsLifeModeActivated;
        InitializeComponent();
        // TODO: 修改为绑定+转换器
        ConflictHintContainer.Visibility = IsLyricsIslandLoaded ? Visibility.Visible : Visibility.Collapsed;
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