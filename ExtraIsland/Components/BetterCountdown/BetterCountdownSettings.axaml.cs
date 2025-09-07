using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;

namespace ExtraIsland.Components;

public partial class BetterCountdownSettings : ComponentBase<BetterCountdownConfig> {
    public BetterCountdownSettings() {
        InitializeComponent();
    }
    
    public List<CountdownAccuracy> CountdownAccuracies { get; } = [
        CountdownAccuracy.Day,
        CountdownAccuracy.Hour,
        CountdownAccuracy.Minute,
        CountdownAccuracy.Second
    ];
    
    void TimePicker_OnLoaded(object? sender,RoutedEventArgs e) {
        TimePicker tp = (TimePicker)sender!;
        tp.SelectedTime = Settings.TargetDateTime.TimeOfDay;
    }
    
    void TimePicker_OnSelectedTimeChanged(object? sender,TimePickerSelectedValueChangedEventArgs e) {
        TimePicker tp = (TimePicker)sender!;
        Settings.TargetDateTime = Settings.TargetDateTime.Date.AddMilliseconds(tp.SelectedTime!.Value.TotalMilliseconds);
    }
}