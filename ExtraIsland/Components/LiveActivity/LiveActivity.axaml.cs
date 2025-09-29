using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ClassIsland.Platforms.Abstraction;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

[ComponentInfo(
    "D61B565D-5BC9-4330-B848-2EDB22F9756E",
    "当前活动",
    "\uF48B",
    "展示前台窗口标题"
)]
public partial class LiveActivity : ComponentBase<LiveActivityConfig> {
    public LiveActivity(ILessonsService lessonsService) {
        LessonsService = lessonsService;
        IsLyricsIslandLoaded = EiUtils.IsLyricsIslandInstalled();
        InitializeComponent();
        _labelAnimator = new Animators.GenericContentSwapAnimator(CurrentLabel);
        _activityAnimator = new Animators.EmphasizerVisualAnimator(ActivityStack);
        _lyricsAnimator = new Animators.EmphasizerVisualAnimator(LyricsStack);
        _lyricsLabelAnimator = new Animators.GenericContentSwapAnimator(LyricsLabel,-0.2);
    }

    ILessonsService LessonsService { get; }
    readonly Animators.GenericContentSwapAnimator _labelAnimator;
    readonly Animators.EmphasizerVisualAnimator _activityAnimator;
    readonly Animators.EmphasizerVisualAnimator _lyricsAnimator;
    readonly Animators.GenericContentSwapAnimator _lyricsLabelAnimator;
    ILyricsProvider? _lyricsHandler;
    string _postName = string.Empty;

    bool IsLyricsIslandLoaded { get; }

    void Check(object? sender,EventArgs eventArgs) {
        Check();
    }

    void Check() {
        Dispatcher.UIThread.InvokeAsync(() => {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Icon.Foreground = WindowsUtils.IsOurWindowInForeground()
                    ? Brushes.DeepSkyBlue
                    : Brushes.LightGreen; 
            } else {
                Icon.Foreground = Brushes.LightGreen;
            }
            string? title = PlatformServices.WindowPlatformService.GetWindowTitle(PlatformServices.WindowPlatformService
                                                                                      .ForegroundWindowHandle);
            title = Replacer(title,Settings.ReplacementsList);
            title = title == "" ? null : title;
            if (title == null & (!Settings.IsLyricsEnabled | _timeCounter <= 0)) {
                CardChip.IsVisible = false;
            } else {
                CardChip.IsVisible = true;
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
        Dispatcher.UIThread.InvokeAsync(() => {
            CardChip.Margin = new Thickness(Settings.IsLeftNegativeMargin ? -12 : 0,
                                            0,
                                            Settings.IsRightNegativeMargin ? -12 : 0,
                                            0);
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
                GlobalConstants.Handlers.LyricsIsland ??= new LyricsIslandLyricsProvider();
                _lyricsHandler = GlobalConstants.Handlers.LyricsIsland;   
            }
            _lyricsHandler.OnLyricsChanged += UpdateLyrics;
            new Thread(() => {
                while (true) {
                    if (!Settings.IsLyricsEnabled) {
                        break;
                    }
                    Dispatcher.UIThread.InvokeAsync(() => {
                        ActivityStack.IsVisible = ActivityStack.Opacity != 0;
                        LyricsStack.IsVisible = LyricsStack.Opacity != 0;
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
            Dispatcher.UIThread.InvokeAsync(() => {
                ActivityStack.IsVisible = true;
                LyricsStack.IsVisible = false;
            });
        }
    }

    double _timeCounter;

    void UpdateLyrics() {
        if (_lyricsHandler == null) return;
        ShowLyrics(true);
        _timeCounter = 10;
        _lyricsLabelAnimator.Update(_lyricsHandler.Lyrics,isForced:true);
    }

    bool _isLyricsShowed;
    void ShowLyrics(bool isShow) {
        if (_isLyricsShowed == isShow) return;
        _isLyricsShowed = isShow;
        _lyricsAnimator.Update(!isShow);
        _activityAnimator.Update(isShow);
    }

    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
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
    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
        LessonsService.PostMainTimerTicked -= Check;
        Settings.OnMarginChanged -= UpdateMargin;
        Settings.OnLyricsChanged -= InitializeLyrics;
        if (_lyricsHandler != null) {
            _lyricsHandler.OnLyricsChanged -= UpdateLyrics;
        }
    }
}