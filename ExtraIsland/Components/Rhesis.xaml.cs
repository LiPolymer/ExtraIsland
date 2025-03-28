using System.Windows;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using Google.Protobuf;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
    "FBB380C2-5480-4FED-8349-BA5F4EAD2688",
    "名句一言",
    PackIconKind.MessageOutline,
    "显示一句古今名言，可使用多种API和在线TXT文件"
)]
public partial class Rhesis
{
    public Rhesis(ILessonsService lessonsService)
    {
        LessonsService = lessonsService;
        InitializeComponent();
        _labelAnimator = new Animators.ClockTransformControlAnimator(Label);
    }

    ILessonsService LessonsService { get; }

    public string Showing { get; private set; } = "-----------------";
    readonly RhesisHandler.Instance _rhesisHandler = new RhesisHandler.Instance();
    readonly Animators.ClockTransformControlAnimator _labelAnimator;
    private Thread? _prefetchThread;
    private bool _isPrefetchRunning = false;

    void Rhesis_OnLoaded(object sender, RoutedEventArgs e)
    {
        Settings.LastUpdate = DateTime.Now;
        Update();
        LessonsService.PostMainTimerTicked += UpdateEvent;

        // Start prefetching immediately regardless of update interval
        StartPrefetching();
    }

    void Rhesis_OnUnloaded(object sender, RoutedEventArgs e)
    {
        LessonsService.PostMainTimerTicked -= UpdateEvent;
        _isPrefetchRunning = false; // Signal prefetch thread to stop
    }

    void UpdateEvent(object? sender, EventArgs eventArgs)
    {
        if (EiUtils.GetDateTimeSpan(Settings.LastUpdate, DateTime.Now) < Settings.UpdateTimeGap | Settings.UpdateTimeGapSeconds == 0) return;
        Settings.LastUpdate = DateTime.Now;
        Update();
    }

    void StartPrefetching()
    {
        if (_prefetchThread != null && _prefetchThread.IsAlive)
        {
            return; // Prefetch thread already running
        }

        _isPrefetchRunning = true;
        _prefetchThread = new Thread(() => {
            while (_isPrefetchRunning)
            {
                try
                {
                    // Always prefetch quotes regardless of update interval
                    RhesisHandler.PrefetchManager.PrefetchQuotes(
                        Settings.EnableHitokoto,
                        Settings.EnableJinrishici,
                        Settings.EnableSaint,
                        Settings.EnableOnlineTxt,
                        Settings.MaxPrefetchedQuotes,
                        Settings.PrefetchIntervalSeconds,
                        Settings.HitokotoProp switch
                        {
                            "" => "https://v1.hitokoto.cn/",
                            _ => $"https://v1.hitokoto.cn/?{Settings.HitokotoLengthArgs}{Settings.HitokotoProp}"
                        },
                        Settings.SainticProp switch
                        {
                            "" => "https://open.saintic.com/api/sentence/",
                            _ => $"https://open.saintic.com/api/sentence/{Settings.SainticProp}.json"
                        },
                        Settings.OnlineTxtUrl
                    );

                    // Sleep for the prefetch interval
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Max(1, Settings.PrefetchIntervalSeconds)));
                }
                catch
                {
                    // Ignore prefetch thread errors
                    Thread.Sleep(TimeSpan.FromSeconds(5)); // Wait a bit if there's an error
                }
            }
        })
        {
            IsBackground = true // Make sure the thread doesn't prevent app from closing
        };

        _prefetchThread.Start();
    }

    void Update()
    {
        new Thread(() => {
            // Calculate the source weights based on settings
            var sourceWeights = new Dictionary<RhesisDataSource, int>();

            if (Settings.EnableHitokoto)
            {
                sourceWeights.Add(RhesisDataSource.Hitokoto, Settings.HitokotoWeight);
            }

            if (Settings.EnableJinrishici)
            {
                sourceWeights.Add(RhesisDataSource.Jinrishici, Settings.JinrishiciWeight);
            }

            if (Settings.EnableSaint)
            {
                sourceWeights.Add(RhesisDataSource.Saint, Settings.SaintWeight);
            }

            if (Settings.EnableOnlineTxt && !string.IsNullOrEmpty(Settings.OnlineTxtUrl))
            {
                sourceWeights.Add(RhesisDataSource.OnlineTxt, Settings.OnlineTxtWeight);
            }

            // If no sources are enabled, default to all
            RhesisDataSource dataSource = sourceWeights.Count > 0 ? RhesisDataSource.All : Settings.DataSource;

            var result = _rhesisHandler.LegacyGet(
                dataSource,
                Settings.HitokotoProp switch
                {
                    "" => "https://v1.hitokoto.cn/",
                    _ => $"https://v1.hitokoto.cn/?{Settings.HitokotoLengthArgs}{Settings.HitokotoProp}"
                },
                Settings.SainticProp switch
                {
                    "" => "https://open.saintic.com/api/sentence/",
                    _ => $"https://open.saintic.com/api/sentence/{Settings.SainticProp}.json"
                },
                Settings.OnlineTxtUrl,
                Settings.LengthLimitation,
                Settings.OnlineTxtWeight,
                sourceWeights.Count > 0 ? sourceWeights : null,
                Settings.EnableHitokoto,
                Settings.EnableJinrishici,
                Settings.EnableSaint,
                Settings.EnableOnlineTxt
            );

            Showing = result.Content;

            this.BeginInvoke(() => {
                if (Settings.IgnoreListString.Split("\r\n").Any(keyWord => Showing.Contains(keyWord) & keyWord != "")) return;
                _labelAnimator.Update(Showing, Settings.IsAnimationEnabled, Settings.IsSwapAnimationEnabled);
            });
        }).Start();
    }
}
