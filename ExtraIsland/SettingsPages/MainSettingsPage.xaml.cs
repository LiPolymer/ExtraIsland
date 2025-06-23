using System.Windows;
using System.Windows.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
// ReSharper disable once RedundantUsingDirective
using ClassIsland.Core;
using ClassIsland.Core.Controls;
using ClassIsland.Core.Controls.CommonDialog;
using ExtraIsland.StandaloneViews;
using MahApps.Metro.Controls;

namespace ExtraIsland.SettingsPages;

[SettingsPageInfo("extraisland.master","ExtraIsland·主设置",PackIconKind.BoxAddOutline,PackIconKind.BoxAdd)]
public partial class MainSettingsPage {
    public MainSettingsPage() {
        Settings = GlobalConstants.Handlers.MainConfig!.Data;
        InitializeComponent();

        if (Settings.IsLifeModeActivated) {
            LifeModeCard.Description = "太有生活了哥们!";
        }

        if (Settings.IsExperimentalModeActivated) {
            DockSettingsCard.Visibility = Visibility.Visible;
        }

        if (EiUtils.IsLyricsIslandInstalled()) {
            //((Chip)LyricsStatCard.Switcher!).Background = Brushes.LightSkyBlue;
            //((Chip)LyricsStatCard.Switcher!).Content = "禁用"; 
            //((Chip)LyricsStatCard.Switcher!).Foreground = Brushes.DarkSlateGray;
            ((Chip)LyricsStatCard.Switcher!).Background = Brushes.Gray;
            ((Chip)LyricsStatCard.Switcher!).Content = "未使用";
        } else if (EiUtils.IsPluginInstalled("ink.lipoly.ext.lychee")) {
            ((Chip)LyricsStatCard.Switcher!).Background = Brushes.Gray;
            ((Chip)LyricsStatCard.Switcher!).Content = "Lychee";
        }
        else if (GlobalConstants.Handlers.LyricsIsland == null) {
            ((Chip)LyricsStatCard.Switcher!).Background = Brushes.Gray;
            ((Chip)LyricsStatCard.Switcher!).Content = "未使用";
        } else {
            ((Chip)LyricsStatCard.Switcher!).Background = GlobalConstants.Handlers.LyricsIsland.Status
                ? Brushes.LightGreen
                : Brushes.IndianRed;
            ((Chip)LyricsStatCard.Switcher!).Content = GlobalConstants.Handlers.LyricsIsland.Status
                ? "正常"
                : "错误";
            ((Chip)LyricsStatCard.Switcher!).Foreground = GlobalConstants.Handlers.LyricsIsland.Status
                ? Brushes.DarkOliveGreen
                : Brushes.White;
            if (!GlobalConstants.Handlers.LyricsIsland.Status) {
                MessageZone.Visibility = Visibility.Visible;
                ErrorMessage.Content = GlobalConstants.Handlers.LyricsIsland.LogMessage;
            }
        }
        Settings.RestartPropertyChanged += SettingsOnPropertyChanged;
    }
    public MainConfigData Settings { get; set; }
    void SettingsOnPropertyChanged() {
        RequestRestart();
    }

    void DockSwitcher_Click(object sender,RoutedEventArgs e) {
        #if !DEBUG
        if (EiUtils.ConvertVersion(AppBase.AppVersion) <= 160.1) {
            Settings.Dock.Enabled = false;
            CommonDialog.ShowError("本功能只支持1.6.0.2及以上版本的ClassIsland");
            return;
        }
        #endif
        if (Settings.Dock.Enabled) {
            Settings.Dock.Enabled = false;
            Label buttonLabel = new Label {
                Content = "[30]确认",
            };
            Button approveButton = new Button {
                Background = Brushes.Goldenrod,
                BorderBrush = Brushes.Transparent,
                Content = buttonLabel,
                IsEnabled = false
            };
            PopupNotification popup = new StandaloneViews.PopupNotification(350,575,60000) {
                Header = "ClassIsDock · 警告 & 设置向导",
                PackIconControl = new PackIcon {
                    Kind = PackIconKind.Cogs,
                    Margin = new Thickness(0,0,0,2),
                    Height = 25,Width = 25
                },
                Body = new StackPanel {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10,35,10,10),
                    Children = {
                        new StackPanel {
                            Orientation = Orientation.Vertical,
                            Children = {
                                new Label {
                                    Content = "注意 (＃°Д°)",
                                    FontSize = 30
                                },
                                new Label {
                                    Content = """
                                              当前该功能还处于早期开发阶段
                                              不对其稳定性作出任何保证

                                              启用本功能后,
                                              右边必要的设置将被自动修改并保持
                                              若您稍后关闭了这个功能,
                                              部分选项需要您重新调整
                                              本功能暂不支持多显示器,仅能在主显示器上显示

                                              继续前,请确保您已阅读并理解以上内容
                                              """
                                },
                                approveButton
                            }
                        },
                        new ScrollViewer {
                            Content = new StackPanel {
                                Orientation = Orientation.Vertical,
                                Children = {
                                    new SettingsControl {
                                        IconGlyph = PackIconKind.Ruler,
                                        Header = "向下偏移",
                                        Description = "窗口 (右上角)",
                                        Switcher = new TextBox {
                                            Width = 30,
                                            HorizontalContentAlignment = HorizontalAlignment.Center,
                                            IsReadOnly = true,
                                            Text = "0"
                                        }
                                    },
                                    new SettingsControl {
                                        IconGlyph = PackIconKind.MouseMoveUp,
                                        Header = "主界面启用鼠标点击",
                                        Description = "ExtraIsland·微功能",
                                        Switcher = new Label {
                                            Margin = new Thickness(20,0,0,0),
                                            HorizontalContentAlignment = HorizontalAlignment.Left,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            Content = "启用",
                                            Foreground = Brushes.LimeGreen
                                        }
                                    },
                                    new SettingsControl {
                                        IconGlyph = PackIconKind.DockWindow,
                                        Header = "使用原始屏幕尺寸",
                                        Description = "窗口",
                                        Switcher = new Label {
                                            Margin = new Thickness(20,0,0,0),
                                            HorizontalContentAlignment = HorizontalAlignment.Left,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            Content = "启用",
                                            Foreground = Brushes.LimeGreen
                                        }
                                    },
                                    new SettingsControl {
                                        IconGlyph = PackIconKind.MouseMoveDown,
                                        Header = "指针移入淡化",
                                        Description = "窗口",
                                        Switcher = new Label {
                                            Margin = new Thickness(20,0,0,0),
                                            HorizontalContentAlignment = HorizontalAlignment.Left,
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            Content = "禁用",
                                            Foreground = Brushes.OrangeRed
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                IconCard = {
                    Background = new SolidColorBrush(Colors.Goldenrod)
                }
            };
            approveButton.Click += (_,_) => {
                Settings.Dock.Enabled = true;
                GlobalConstants.Handlers.MainWindow!.InitBar(accentState: Settings.Dock.AccentState);
                popup.FadeOut();
            };
            popup.Show();
            new Thread(() => {
                for (int x = 30; x > 0; x--) {
                    int x1 = x;
                    popup.BeginInvoke(() => {
                        buttonLabel.Content = $"[{x1.ToString()}]确认";
                    });
                    Thread.Sleep(1000);
                }
                popup.BeginInvoke(() => {
                    buttonLabel.Content = $"确认";
                    approveButton.IsEnabled = true;
                });
            }).Start();
        } else {
            MainWindowHandler.TargetConfigs.IsMouseClickingEnabled = false;
            RequestRestart();
        }
    }

    void ExpSwitcher_Click(object sender,RoutedEventArgs e) {
        if (Settings.IsExperimentalModeActivated) {
            Settings.IsExperimentalModeActivated = false;
            Label buttonLabel = new Label {
                Content = "[30]确认",
            };
            Button approveButton = new Button {
                Background = Brushes.OrangeRed,
                BorderBrush = Brushes.Transparent,
                Content = buttonLabel,
                IsEnabled = false
            };
            PopupNotification popup = new StandaloneViews.PopupNotification(350,575,60000) {
                Header = "ExtraIsland · 警告",
                PackIconControl = new PackIcon {
                    Kind = PackIconKind.Warning,
                    Margin = new Thickness(0,0,0,2),
                    Height = 25,Width = 25
                },
                Body = new StackPanel {
                    Margin = new Thickness(10,35,10,10),
                    Orientation = Orientation.Vertical,
                    Children = {
                        new Label {
                            Content = "稳定性警告 o(≧口≦)o",
                            FontSize = 30
                        },
                        new Label {
                            FontSize = 20,
                            Content = """
                                      
                                      当前这些功能还处于早期开发阶段
                                      存在不少已知Bug        （；´д｀）ゞ
                                      考虑到部分功能有用户仍在使用
                                      故将本选项在正式版中继续保留 (至少能用不是吗 (●'◡'●) )
                                      若使用这些功能的时候遇到问题,请勿到仓库提交Bug!
                                      """
                        },
                        approveButton
                    }
                },
                IconCard = {
                    Background = new SolidColorBrush(Colors.OrangeRed)
                }
            };
            approveButton.Click += (_,_) => {
                Settings.IsExperimentalModeActivated = true;
                DockSettingsCard.Visibility = Visibility.Visible;
                popup.Topmost = false;
                CommonDialog.ShowHint("重启应用以激活部分组件内容");
                popup.FadeOut();
            };
            popup.Show();
            new Thread(() => {
                for (int x = 30; x > 0; x--) {
                    int x1 = x;
                    popup.BeginInvoke(() => {
                        buttonLabel.Content = $"[{x1.ToString()}]确认";
                    });
                    Thread.Sleep(1000);
                }
                popup.BeginInvoke(() => {
                    buttonLabel.Content = "确认";
                    approveButton.IsEnabled = true;
                });
            }).Start();
        } else {
            DockSettingsCard.Visibility = Visibility.Collapsed;
            MainWindowHandler.TargetConfigs.IsMouseClickingEnabled = false;
            Settings.Dock.Enabled = false;
            RequestRestart();
        }
    }

    public List<MainWindowHandler.AccentHelper.AccentState> AccentStates { get; } = [
        MainWindowHandler.AccentHelper.AccentState.AccentEnableBlurbehind,
        MainWindowHandler.AccentHelper.AccentState.AccentEnableTransparentgradient,
        MainWindowHandler.AccentHelper.AccentState.AccentDisabled,
    ];
}