using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
    "D61B565D-5BC9-4330-B848-2EDB22F9756E",
    "当前活动",
    PackIconKind.ApplicationSettingsOutline,
    "展示前台窗口标题"
)]
public partial class LiveActivity {
    public LiveActivity(ILessonsService lessonsService) {
        LessonsService = lessonsService;
        IsLyricsIslandLoaded = EiUtils.IsLyricsIslandInstalled();
        InitializeComponent();
        _labelAnimator = new Animators.ClockTransformControlAnimator(CurrentLabel);
        _activityAnimator = new Animators.EmphasizeUiElementAnimator(ActivityStack,5);
        _lyricsAnimator = new Animators.EmphasizeUiElementAnimator(LyricsStack,5);
        _lyricsLabelAnimator = new Animators.ClockTransformControlAnimator(LyricsLabel,-0.2);
    }

    ILessonsService LessonsService { get; }
    readonly Animators.ClockTransformControlAnimator _labelAnimator;
    readonly Animators.EmphasizeUiElementAnimator _activityAnimator;
    readonly Animators.EmphasizeUiElementAnimator _lyricsAnimator;
    readonly Animators.ClockTransformControlAnimator _lyricsLabelAnimator;
    ILyricsProvider? _lyricsHandler;
    string _postName = string.Empty;

    bool IsLyricsIslandLoaded { get; }

    void Check(object? sender,EventArgs eventArgs) {
        Check();
    }

    void Check() {
        this.BeginInvoke(() => {
            Icon.Foreground = WindowsUtils.IsOurWindowInForeground()
                ? Brushes.DeepSkyBlue
                : Brushes.LightGreen;
            string? title = WindowsUtils.GetActiveWindowTitle();
            title ??= "";
            title = Replacer(title,Settings.ReplacementsList);
            title = title == "" ? null : title;
            if (title == null & (!Settings.IsLyricsEnabled | _timeCounter <= 0)) {
                CardChip.Visibility = Visibility.Collapsed;
            } else {
                CardChip.Visibility = Visibility.Visible;
                if (title != null) {
                    _labelAnimator.Update(title.Replace("_","__"),Settings.IsAnimationEnabled,false);
                    if (Settings.IsSleepyUploaderEnabled) {
                        string appName = string.Format(Settings.SleepyPattern,title);
                        if (_postName != appName) {
                            _postName = appName;
                            new Thread(() => {
                                new SleepyHandler.SleepyApiData.PostData {
                                    AppName = appName,
                                    Id = Settings.SleepyDeviceId,
                                    Secret = Settings.SleepySecret,
                                    ShowName = Settings.SleepyDevice,
                                    Using = true
                                }.Post(Settings.SleepyUrl);
                            }).Start();
                        }
                    }
                }
            }
        });
    }

    string Replacer(string input, IEnumerable<ReplaceItem> replaceList) {
        foreach (ReplaceItem kvp in replaceList) {
            try {
                Regex regex = new Regex(kvp.Regex);
                if (regex.IsMatch(input)) {
                    return regex.Replace(input,kvp.Replacement);
                }
            }
            catch {
                //ignored
            }
        }
        return input;
    }
    
    void UpdateMargin() {
        this.BeginInvoke(() => {
            CardChip.Margin = new Thickness {
                Top = 0,
                Bottom = 0,
                Left = Settings.IsLeftNegativeMargin ? -12 : 0,
                Right = Settings.IsRightNegativeMargin ? -12 : 0
            };
        });
    }

    void InitializeLyrics() {
        if (IsLyricsIslandLoaded) {
            Settings.IsLyricsEnabled = false;
            return;
        }
        if (Settings.IsLyricsEnabled) {
            if (EiUtils.IsPluginInstalled("ink.lipoly.ext.lychee")) {
                _lyricsHandler = new LycheeLyricsProvider();
            } else {
                GlobalConstants.Handlers.LyricsIsland ??= new LyricsIslandHandler();
                _lyricsHandler = GlobalConstants.Handlers.LyricsIsland;   
            }
            _lyricsHandler.OnLyricsChanged += UpdateLyrics;
            new Thread(() => {
                while (true) {
                    if (!Settings.IsLyricsEnabled) {
                        break;
                    }
                    this.BeginInvoke(() => {
                        ActivityStack.Visibility = ActivityStack.Opacity == 0 ? Visibility.Collapsed : Visibility.Visible;
                        LyricsStack.Visibility = LyricsStack.Opacity == 0 ? Visibility.Collapsed : Visibility.Visible;
                    });
                    if (_timeCounter > 0) {
                        _timeCounter -= 0.01;
                    } else {
                        ShowLyrics(false);
                    }
                    Thread.Sleep(10);
                }
            }).Start();
        } else {
            if (_lyricsHandler == null) return;
            _lyricsHandler.OnLyricsChanged -= UpdateLyrics;
            _lyricsHandler = null;
            ShowLyrics(false);
            this.BeginInvoke(() => {
                ActivityStack.Visibility = Visibility.Visible;
                LyricsStack.Visibility = Visibility.Collapsed;
            });
        }
    }

    double _timeCounter;

    void UpdateLyrics() {
        this.BeginInvoke(() => {
            if (_lyricsHandler == null) return;
            ShowLyrics(true);
            _timeCounter = 10;
            _lyricsLabelAnimator.Update(_lyricsHandler.Lyrics,isForced:true);
        });
    }

    bool _isLyricsShowed;
    void ShowLyrics(bool isShow) {
        if (_isLyricsShowed == isShow) return;
        this.BeginInvoke(() => {
            _isLyricsShowed = isShow;
            _lyricsAnimator.Update(!isShow);
            _activityAnimator.Update(isShow);
        });
    }

    void LiveActivity_OnLoaded(object sender,RoutedEventArgs e) {
        //配置迁移
        if (Settings.IgnoreListString != string.Empty) {
            string[] oldList;
            try {
                oldList = Settings.IgnoreListString.Split("\r\n");
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                oldList = [];
            }
            foreach (string item in oldList) {
                Settings.ReplacementsList.Add(new ReplaceItem {
                    Regex = $"^{Regex.Escape(item)}$",
                    Replacement = string.Empty
                });
            }
            Settings.IgnoreListString = string.Empty;
        }
        
        if (!GlobalConstants.Handlers.MainConfig!.Data.IsLifeModeActivated) {
            Settings.IsSleepyUploaderEnabled = false;
        }
        UpdateMargin();
        InitializeLyrics();
        LessonsService.PostMainTimerTicked += Check;
        Settings.OnMarginChanged += UpdateMargin;
        Settings.OnLyricsChanged += InitializeLyrics;
    }
    void LiveActivity_OnUnloaded(object sender,RoutedEventArgs e) {
        LessonsService.PostMainTimerTicked -= Check;
        Settings.OnMarginChanged -= UpdateMargin;
        Settings.OnLyricsChanged -= InitializeLyrics;
        if (_lyricsHandler != null) {
            _lyricsHandler.OnLyricsChanged -= UpdateLyrics;
        }
    }
}