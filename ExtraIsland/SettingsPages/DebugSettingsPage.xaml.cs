using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClassIsland.Core;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using CommonDialog = ClassIsland.Core.Controls.CommonDialog.CommonDialog;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;

namespace ExtraIsland.SettingsPages;

[SettingsPageInfo("extraisland.debug", "ExtraIsland·调试", PackIconKind.WrenchOutline, PackIconKind.Wrench)]
public partial class DebugSettingsPage {
    public DebugSettingsPage() {
        InitializeComponent();
    }
    void PopButton_OnClick(object sender,RoutedEventArgs e) {
        StandaloneViews.PopupNotification popup = new StandaloneViews.PopupNotification(350,575,PopProps.GetHoldTime()) {
            Header = "ExtraIsland·弹出式通知[调试]",
            PackIconControl = new PackIcon {
                Kind = PackIconKind.Wrench,
                Height = 25, Width = 25
            },
            Body = new Label {
                Content = "114514",
                FontSize = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        popup.Show();
    }
    
    public PopupDebugProps PopProps { get; set; } = new PopupDebugProps();
    
    [GeneratedRegex("[^0-9.-]+")]
    private static partial Regex NumberRegex();
    void TextBoxNumberCheck(object sender,TextCompositionEventArgs e) {
        Regex re = NumberRegex();
        e.Handled = re.IsMatch(e.Text);
    }
    
    JinrishiciData Jinrishici { get; set; } = new JinrishiciData();
    SainticData Saintic { get; set; } = new SainticData();
    HitokotoData Hitokoto { get; set; } = new HitokotoData();
    void RequestButton_OnClick(object sender,RoutedEventArgs e) {
        Jinrishici  = JinrishiciData.Fetch();
        Saintic = SainticData.Fetch();
        Hitokoto = HitokotoData.Fetch();
        CommonDialog.ShowInfo(message:
            $"今日诗词 | {Jinrishici.ToRhesisData().Title} - {Jinrishici.ToRhesisData().Author}"
            + "\r\n         |" + Jinrishici.ToRhesisData().Catalog
            + "\r\n         |" + Jinrishici.ToRhesisData().Content
            + "\r\n"
            + "\r\n" + $"    诏预 | {Saintic.ToRhesisData().Title} - {Saintic.ToRhesisData().Author}"
            + "\r\n         |" + Saintic.ToRhesisData().Catalog
            + "\r\n         |" + Saintic.ToRhesisData().Content
            + "\r\n"
            + "\r\n" + $"    一言 | {Hitokoto.ToRhesisData().Title} - {Hitokoto.ToRhesisData().Author}"
            + "\r\n         |" + Hitokoto.ToRhesisData().Catalog
            + "\r\n         |" + Hitokoto.ToRhesisData().Content);
    }
    
    void RandRequestButton_OnClick(object sender,RoutedEventArgs e) {
        RhesisData rhesis = new RhesisHandler.Instance().LegacyGet();
        CommonDialog.ShowInfo(message:
            $"{rhesis.Source} | {rhesis.Title} - {rhesis.Author}"
            + "\r\n         |" + rhesis.Catalog
            + "\r\n         |" + rhesis.Content);
    }
    void MainWindowTransform_OnClick(object sender,RoutedEventArgs e) {
        this.BeginInvoke(() => {
            Animators.IslandDriftAnimator islandDriftAnimator = new Animators.IslandDriftAnimator(AppBase.Current.MainWindow!, 
                Color.FromArgb(0xee,0x00,0x00,0x00), 100);
            //islandDriftAnimator.Expand(true);
            islandDriftAnimator.Background(true);
        });
    }
    void ClassIsDock(object sender,RoutedEventArgs eventArgs) {
        MainWindowHandler bar = GlobalConstants.Handlers.MainWindow!;
        bar.SetBar();
    }
}

public class PopupDebugProps {
    public string Height { get; set; } = "350";
    public string Width { get; set; } = "575";
    public string HoldTime { get; set; } = "5000";

    public int GetHeight() {
        return Convert.ToInt32(Height);
    }
    public int GetWidth() {
        return Convert.ToInt32(Width);
    }
    public int GetHoldTime() {
        return Convert.ToInt32(HoldTime);
    }
}