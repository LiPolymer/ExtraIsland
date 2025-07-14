using Avalonia.Interactivity;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

// TODO: 调试结束后移除此组件
[ComponentInfo(
    "D61B565D-5BC9-9999-6666-2EDB22F9756E",
    "调试 · 歌词",
    "\uEBCA",
    "测试歌词岛接口封装类LyricsIslandHandler()"
)]

// ReSharper disable once ClassNeverInstantiated.Global
public partial class DebugLyricsHandler: ComponentBase {
    public DebugLyricsHandler() {
        if (EiUtils.IsPluginInstalled("ink.lipoly.ext.lychee")) {
            _handler = new LycheeLyricsProvider();
        } else {
            GlobalConstants.Handlers.LyricsIsland ??= new LyricsIslandLyricsProvider();
            _handler = GlobalConstants.Handlers.LyricsIsland;   
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
        _nowDisplaying = _handler.Lyrics;
        _animator.Update(_nowDisplaying,true,true,true);
    }

    string _nowDisplaying = string.Empty;
    void CounterDaemon() {
        while (_daemonEnabled) {
            _timeCounter -= 1;
            if (_timeCounter <= 0 & _nowDisplaying != string.Empty) {
                _nowDisplaying = string.Empty;
                _animator.Update(_nowDisplaying);
            }
            Thread.Sleep(1000);
        }
    }
    bool _daemonEnabled;
    void Control_OnLoaded(object? sender,RoutedEventArgs e) {
        _daemonEnabled = true;
        new Thread(CounterDaemon).Start();
    }
    void Control_OnUnloaded(object? sender,RoutedEventArgs e) {
        _daemonEnabled = false;
    }
}