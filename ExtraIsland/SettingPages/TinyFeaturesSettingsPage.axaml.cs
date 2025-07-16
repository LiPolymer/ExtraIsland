using Avalonia.Interactivity;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;

namespace ExtraIsland.SettingsPages;

[SettingsPageInfo("extraisland.tiny","ExtraIsland·微功能","\uEDC5","\uEDC4")]
public partial class TinyFeaturesSettingsPage : SettingsPageBase {
    public TinyFeaturesSettingsPage() {
        Settings = GlobalConstants.Handlers.MainConfig!.Data.TinyFeatures;
        InitializeComponent();
        dynamic app = AppBase.Current;
        AppSettings = app.Settings;
    }

    public MainConfigData.TinyFeaturesConfig Settings { get; set; }
    
    public dynamic AppSettings { get; set; }
    
    void MiscSettingsPage_OnUnloaded(object sender,RoutedEventArgs e) {
        
    }
    void DebugShowButton_OnClick(object sender,RoutedEventArgs e) {
        //JuniorGuide.Show(true);
    }
    void ToggleButton_OnChecked(object sender,RoutedEventArgs e) {
        //todo: 恢复警示弹窗
        /*
        if (AppSettings.IsMouseClickingEnabled != true) return;
        int result = CommonDialog.ShowHint("警告:\r\n"
                                           + " 这个功能是 ClassIsland 没有正式使用的功能,不保证能正常工作\r\n"
                                           + " 启用这个功能,会导致主界面鼠标穿透失效\r\n"
                                           + " 请谨慎开启!");
        if (result != 0) {
            AppSettings.IsMouseClickingEnabled = false;
        }
        */
    }
}