using System.Windows;
using System.Windows.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
                  "FBB380C2-5480-4FED-8349-BA5F4EAD2688",
                  "名句一言",
                  PackIconKind.MessageOutline,
                  "显示一句古今名言,可使用三个API"
              )]
public partial class Rhesis {
    public Rhesis(ILessonsService lessonsService) {
        _authorLabel = new Label {
            Content = Title,
            Margin = new Thickness(0,4,0,0),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_authorLabel,0);
        _authorLabel.SetResourceReference(FontSizeProperty, "MainWindowSecondaryFontSize");

        _titleLabel = new Label {
            Content = Author,
            Margin = new Thickness(0,0,0,4),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_titleLabel,1);
        _titleLabel.SetResourceReference(FontSizeProperty, "MainWindowSecondaryFontSize");
        
        _infoGrid = new Grid {
            VerticalAlignment = VerticalAlignment.Center,
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            Children = {
                _authorLabel,
                _titleLabel
            }
        };
        
        LessonsService = lessonsService;
        InitializeComponent();
        _mainLabelAnimator = new Animators.ClockTransformControlAnimator(MainLabel);
        _subLabelAnimator = new Animators.ClockTransformControlAnimator(SubLabel);
    }

    ILessonsService LessonsService { get; }

    public string Showing { get; private set; } = "-----------------";
    public string Author { get; private set; } = "";
    public string Title { get; private set; } = "";
    readonly RhesisHandler.Instance _rhesisHandler = new RhesisHandler.Instance();
    readonly Animators.ClockTransformControlAnimator _mainLabelAnimator;
    readonly Animators.ClockTransformControlAnimator _subLabelAnimator;
    readonly Grid _infoGrid;
    readonly Label _titleLabel;
    readonly Label _authorLabel;
    
    void Rhesis_OnLoaded(object sender,RoutedEventArgs e) {
        Settings.LastUpdate = DateTime.Now;
        Update();
        LessonsService.PostMainTimerTicked += UpdateEvent;
    }

    void Rhesis_OnUnloaded(object sender,RoutedEventArgs e) {
        LessonsService.PostMainTimerTicked -= UpdateEvent;
    }

    void UpdateEvent(object? sender,EventArgs eventArgs) {
        if (EiUtils.GetDateTimeSpan(Settings.LastUpdate,DateTime.Now) < Settings.UpdateTimeGap
            | Settings.UpdateTimeGapSeconds == 0) return;
        Settings.LastUpdate = DateTime.Now;
        Update();
    }

    void Update() {
        Task.Run(() => {
            RhesisData data = _rhesisHandler.LegacyGet(Settings.DataSource,Settings.HitokotoProp switch {
                                                           "" => "https://v1.hitokoto.cn/",
                                                           _ => $"https://v1.hitokoto.cn/?{Settings.HitokotoLengthArgs}{Settings.HitokotoProp}"
                                                       },
                                                       Settings.SainticProp switch {
                                                           "" => "https://open.saintic.com/api/sentence/",
                                                           _ => $"https://open.saintic.com/api/sentence/{Settings.HitokotoProp}.json"
                                                       },
                                                       Settings.LengthLimitation);
            Showing = data.Content;
            Title = data.Title;
            Author = data.Author;
            if (Settings.IgnoreListString.Split("\r\n").Any(keyWord => Showing.Contains(keyWord) && keyWord != "")) return;
            object subObj;
            if (Settings.IsAuthorShowEnabled & Settings.IsTitleShowEnabled) {
                if (Settings.AttributesShowingInterval == 0) {
                    subObj = _infoGrid;   
                } else {
                    subObj = $"{Author} {Title}";
                }
            } else if (Settings.IsAuthorShowEnabled) {
                subObj = Author;
            } else if (Settings.IsTitleShowEnabled) {
                subObj = Title;
            } else {
                subObj = string.Empty;
            }
            this.BeginInvoke(() => {
                _titleLabel.Content = Title;
                _authorLabel.Content = Author;
                _mainLabelAnimator.Update(Showing,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
                if (Settings.IsAuthorShowEnabled | Settings.IsTitleShowEnabled) {
                    if (Settings.AttributesShowingInterval == 0) {
                        SubLabel.Visibility =  Visibility.Visible;
                        _subLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
                    } else {
                        SubLabel.Visibility =  Visibility.Collapsed;
                        if (Settings.AttributesShowingInterval > (Settings.UpdateTimeGapSeconds - 3)) {
                            Settings.AttributesShowingInterval = 0;
                        } else { 
                            new Thread(() => {
                                Thread.Sleep(Settings.AttributesShowingInterval * 1000);
                                this.BeginInvoke(() => {
                                    _mainLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,false); 
                                });
                            }).Start();
                        }
                    }
                } else {
                    SubLabel.Visibility =  Visibility.Collapsed;
                }
            });
        });
    }

    StackPanel GenerateInfoStack() {
        return new StackPanel {
            Orientation = Orientation.Vertical,
            Children = {
                new Label {
                    Content = Title,
                    Padding = new Thickness(0),
                    FontSize = (double)FindResource("MainWindowSecondaryFontSize") - 1.5
                },
                new Label {
                    Content = Author,
                    Padding = new Thickness(0),
                    FontSize = (double)FindResource("MainWindowSecondaryFontSize") - 1.5
                }
            }
        };
    }

    Grid GenerateInfoGrid() {
        Grid mainGrid = new Grid {
            VerticalAlignment = VerticalAlignment.Center,
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
        };

        Label dualLabelUp = new Label {
            Content = Title,
            Margin = new Thickness(0,4,0,0),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        Grid.SetRow(dualLabelUp,0);

        Label dualLabelDown = new Label {
            Content = Author,
            Margin = new Thickness(0,0,0,4),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        Grid.SetRow(dualLabelDown,1);
        mainGrid.Children.Add(dualLabelUp);
        mainGrid.Children.Add(dualLabelDown);
        
        dualLabelUp.SetResourceReference(FontSizeProperty, "MainWindowSecondaryFontSize");
        dualLabelDown.SetResourceReference(FontSizeProperty, "MainWindowSecondaryFontSize");
        return mainGrid;
    }
}