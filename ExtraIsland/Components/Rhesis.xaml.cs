using System.Windows;
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
        LessonsService = lessonsService;
        InitializeComponent();
        _labelAnimator1 = new Animators.ClockTransformControlAnimator(Label1);
        _labelAnimator2 = new Animators.ClockTransformControlAnimator(Label2);
        _labelAnimator3 = new Animators.ClockTransformControlAnimator(Label3);
    }

    ILessonsService LessonsService { get; }

    public string Showing { get; private set; } = "-----------------";
    public string Author { get; private set; }
    public string Title { get; private set; }
    readonly RhesisHandler.Instance _rhesisHandler = new RhesisHandler.Instance();
    readonly Animators.ClockTransformControlAnimator _labelAnimator1;
    readonly Animators.ClockTransformControlAnimator _labelAnimator2;
    readonly Animators.ClockTransformControlAnimator _labelAnimator3;
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
            var data = _rhesisHandler.LegacyGet(Settings.DataSource,Settings.HitokotoProp switch {
                    "" => "https://v1.hitokoto.cn/",
                    _ => $"https://v1.hitokoto.cn/?{Settings.HitokotoLengthArgs}{Settings.HitokotoProp}"
                },
                Settings.SainticProp switch {
                    "" => "https://open.saintic.com/api/sentence/",
                    _ => $"https://open.saintic.com/api/sentence/{Settings.HitokotoProp}.json"
                },
                Settings.LengthLimitation);
            Showing = data.Content;
            if (Settings.IgnoreListString.Split("\r\n").Any(keyWord => Showing.Contains(keyWord) && keyWord != "")) return;
            if (Settings.IsAuthorShowEnabled && data.Author != null && data.Author != string.Empty) {
                Author = $"{data.Author}";
                this.BeginInvoke(() => Label2.Visibility = Visibility.Visible);
            } else {
                Author = string.Empty;
                this.BeginInvoke(() => Label2.Visibility = Visibility.Hidden);
            }
            if (Settings.IsTitleShowEnabled && data.Title != null && data.Title != string.Empty) {
                Title = $"{data.Title}";
                this.BeginInvoke(() => Label3.Visibility = Visibility.Visible);
            } else {
                Title = string.Empty;
                this.BeginInvoke(() => Label3.Visibility = Visibility.Hidden);
            }
            this.BeginInvoke(() => {
                _labelAnimator1.Update(Showing,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
            });
            this.BeginInvoke(() => {
                _labelAnimator2.Update(Author,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
            });
            this.BeginInvoke(() => {
                _labelAnimator3.Update(Title,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
            });

        });
    }
}