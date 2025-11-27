using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;

public partial class LaterThan : RuleSettingsControlBase<LaterThanConfig> {
    public LaterThan() {
        InitializeComponent();
    }
    
    public static bool Rule(object? rawConfig) {
        LaterThanConfig config = (LaterThanConfig)rawConfig!;
        TimeSpan current = GlobalConstants.HostInterfaces.ExactTimeService!.GetCurrentLocalDateTime().TimeOfDay;
        return current.CompareTo(config.TargetTime.TimeOfDay) switch {
            < 0 => false,
            >= 0 => true
        };
    }
    
    void TimePicker_OnLoaded(object? sender,RoutedEventArgs e) {
        TimePicker tp = (TimePicker)sender!;
        tp.SelectedTime = Settings.TargetTime.TimeOfDay;
    }
    
    void TimePicker_OnSelectedTimeChanged(object? sender,TimePickerSelectedValueChangedEventArgs e) {
        TimePicker tp = (TimePicker)sender!;
        Settings.TargetTime = DateTime.Today.Date.AddMilliseconds(tp.SelectedTime!.Value.TotalMilliseconds);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class LaterThanConfig : ObservableRecipient {
    public DateTime TargetTime { get; set; } = DateTime.Now;
}