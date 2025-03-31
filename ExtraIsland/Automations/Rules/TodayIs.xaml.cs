using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls;

namespace ExtraIsland.Automations.Rules;

public partial class TodayIs {
    public TodayIs() {
        InitializeComponent();
    }

    void OnTargetTypeChanged() {
        this.BeginInvoke((() => {
            DayOfWeekSelector.Visibility = Settings.Target == TodayIsConfig.TargetType.DayOfWeek 
                ? Visibility.Visible : Visibility.Collapsed;
        }));
    }

    public List<TodayIsConfig.TargetType> TargetTypes { get; } = [
        TodayIsConfig.TargetType.DayOfWeek,
        TodayIsConfig.TargetType.Weekday,
        TodayIsConfig.TargetType.Weekend
    ];

    public static List<DayOfWeek> DayOfWeeks { get; } = [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];

    static List<DayOfWeek> DayOfWeekdays { get; } = [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    static List<DayOfWeek> DayOfWeekend { get; } = [
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];
    void TodayIs_OnLoaded(object sender,RoutedEventArgs e) {
        OnTargetTypeChanged();
        Settings.TargetTypeChanged += OnTargetTypeChanged;
    }
    void TodayIs_OnUnloaded(object sender,RoutedEventArgs e) {
        Settings.TargetTypeChanged -= OnTargetTypeChanged;
    }
    public static bool Rule(object? rawConfig) {
        TodayIsConfig config = (TodayIsConfig)rawConfig!;
        return config.Target switch {
            TodayIsConfig.TargetType.DayOfWeek => DateTime.Now.DayOfWeek == config.TargetDayOfWeek,
            TodayIsConfig.TargetType.Weekday => DayOfWeekdays.Contains(DateTime.Now.DayOfWeek),
            TodayIsConfig.TargetType.Weekend => DayOfWeekend.Contains(DateTime.Now.DayOfWeek),
            _ => false
        };
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class TodayIsConfig : ObservableRecipient {

    public event Action? TargetTypeChanged;

    TargetType _targetType = TargetType.Weekday;
    public TargetType Target {
        get => _targetType;
        set {
            if (_targetType == value) return;
            _targetType = value;
            TargetTypeChanged?.Invoke();
        }
    }
    public DayOfWeek TargetDayOfWeek { get; set; } = DayOfWeek.Monday;

    public enum TargetType {
        [Description("工作日")]
        Weekday,
        [Description("周末")]
        Weekend,
        [Description("星期...")]
        DayOfWeek
    }
}