using Avalonia;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

[ComponentInfo(
    "23aac481-8b12-4fc9-aac4-818b129fc9a7",
    "动态歌词",
    "\uEBCA",
    "展示歌词, 支持LyricsIsland协议或LycheeLib"
)]

// ReSharper disable once ClassNeverInstantiated.Global
public partial class DynamicLyrics: ComponentBase<DynamicLyricsConfig> {
    public DynamicLyrics(LyricsIslandLyricsProvider lyricsIslandProvider,LycheeLyricsProvider lycheeProvider) {
        if (EiUtils.IsPluginInstalled("ink.lipoly.ext.lychee")) {
            _handler = lycheeProvider;
        } else {
            _handler = lyricsIslandProvider;
        }
        InitializeComponent();
        _handler.OnLyricsChanged += UpdateLyrics;
        _animator = new Animators.GenericContentSwapAnimator(Label,-0.5);
    }

    readonly ILyricsProvider _handler;
    readonly Animators.GenericContentSwapAnimator _animator;

    int _timeCounter = 10;
    
    void UpdateLyrics() {
        _timeCounter = 10;
        _nowDisplaying = Settings.DisplayType switch {
            LyricsDisplayType.MainLine => _handler.Lyrics,
            LyricsDisplayType.SubLine => _handler.SubLyrics,
            _ => throw new ArgumentOutOfRangeException()
        };
        _animator.Update(_nowDisplaying,true,true,true);
    }

    string _nowDisplaying = string.Empty;
    void CounterDaemon() {
        while (_daemonEnabled) {
            _timeCounter -= 1;
            if (_timeCounter <= 0 & _nowDisplaying != string.Empty) {
                _nowDisplaying = string.Empty;
                _animator.Update(_nowDisplaying,true,false);
            }
            Thread.Sleep(1000);
        }
    }
    bool _daemonEnabled;
    void OnDetachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs) {
        _daemonEnabled = false;
    }
    void OnAttachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        _daemonEnabled = true;
        new Thread(CounterDaemon).Start();
    }
}