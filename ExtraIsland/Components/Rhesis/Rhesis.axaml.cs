using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

[ComponentInfo(
                  "FBB380C2-5480-4FED-8349-BA5F4EAD2688",
                  "名句一言",
                  "\uE3F4",
                  "显示一句古今名言，支持可扩展内容来源"
              )]
public partial class Rhesis : ComponentBase<RhesisConfig> {
    public Rhesis(ILessonsService lessonsService,IRhesisService rhesisService,IRhesisProviderRegistry registry) {
        _authorLabel = new Label {
            Content = Title,
            Margin = new Thickness(0,4,0,0),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_authorLabel,0);
        _authorLabel.Bind(FontSizeProperty,new DynamicResourceExtension("MainWindowSecondaryFontSize"));
        //_authorLabel.SetResourceReference(FontSizeProperty, "MainWindowSecondaryFontSize");

        _titleLabel = new Label {
            Content = Author,
            Margin = new Thickness(0,0,0,4),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_titleLabel,1);
        _titleLabel.Bind(FontSizeProperty,new DynamicResourceExtension("MainWindowSecondaryFontSize"));
        //_titleLabel.SetResourceReference(FontSizeProperty, "MainWindowSecondaryFontSize");
        
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
        _rhesisService = rhesisService;
        _registry = registry;
        InitializeComponent();
        _mainLabelAnimator = new Animators.GenericContentSwapAnimator(MainLabel);
        _subLabelAnimator = new Animators.GenericContentSwapAnimator(SubLabel);
    }

    ILessonsService LessonsService { get; }
    readonly IRhesisService _rhesisService;
    readonly IRhesisProviderRegistry _registry;

    public string Showing { get; private set; } = "-----------------";
    public string Author { get; private set; } = "";
    public string Title { get; private set; } = "";
    readonly Animators.GenericContentSwapAnimator _mainLabelAnimator;
    readonly Animators.GenericContentSwapAnimator _subLabelAnimator;
    readonly Grid _infoGrid;
    readonly Label _titleLabel;
    readonly Label _authorLabel;
    
    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
        Settings.LastUpdate = DateTime.Now;
        Update();
        LessonsService.PostMainTimerTicked += UpdateEvent;
    }

    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
        LessonsService.PostMainTimerTicked -= UpdateEvent;
    }

    void UpdateEvent(object? sender,EventArgs eventArgs) {
        if (EiUtils.GetDateTimeSpan(Settings.LastUpdate,DateTime.Now) < Settings.UpdateTimeGap
            | Settings.UpdateTimeGapSeconds == 0) return;
        Settings.LastUpdate = DateTime.Now;
        Update();
    }

    void Update() {
        Task.Run(async () => {
            Settings.EnsureProviderSettings(_registry.Providers);
            RhesisData data = await _rhesisService.GetAsync(
                Settings.ProviderSettings,
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
            Dispatcher.UIThread.InvokeAsync(() => {
                _titleLabel.Content = Title;
                _authorLabel.Content = Author;
                _mainLabelAnimator.Update(Showing,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
                if (Settings.IsAuthorShowEnabled | Settings.IsTitleShowEnabled) {
                    if (Settings.AttributesRule == RhesisConfig.AttributesDisplayRule.Sametime) {
                        SubLabel.IsVisible =  true;
                        _subLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
                    } else {
                        SubLabel.IsVisible = false;
                        if (Math.Abs(Settings.AttributesShowingInterval - Settings.UpdateTimeGapSeconds) < 0.5) {
                            _mainLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,false);
                            return;
                        }
                        new Thread(() => {
                            Thread.Sleep((int)((Settings.UpdateTimeGapSeconds - Settings.AttributesShowingInterval) * 1000));
                            _mainLabelAnimator.Update(subObj,Settings.IsAnimationEnabled,false);
                        }).Start();
                    }
                } else {
                    SubLabel.IsVisible =  false;
                }
            });
        });
    }
}
