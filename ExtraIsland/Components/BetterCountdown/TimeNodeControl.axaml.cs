using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;


namespace ExtraIsland.Components;

public partial class TimeNodeControl : UserControl {
    public TimeNodeControl() {
        InitializeComponent();
    }
    
    //public BetterCountdownConfig Config {get; set;}
    public static readonly StyledProperty<ObservableCollection<TimeNode>> TimesProperty =
        AvaloniaProperty.Register<TimeNodeControl, ObservableCollection<TimeNode>>(nameof(Times));
    
    public ObservableCollection<TimeNode> Times {
        get => GetValue(TimesProperty);
        set => SetValue(TimesProperty, value);
    }
    
    public void ButtonAddTime_Click(object? sender, RoutedEventArgs e) {
        Times.Add(new TimeNode());
    }
    
    [RelayCommand]
    void ButtonRemoveTime(TimeNode node) {
        Times.Remove(node);
    }
    
    void CountdownTimeModeTp_OnLoaded(object? sender,RoutedEventArgs e) {
        if (sender is TimePicker 
            {                                   
                DataContext: TimeNode tn,  
            } tp) {
            Console.WriteLine((tn.CountdownTime.Hours,tn.CountdownTime.Minutes,tn.CountdownTime.Seconds));
            tp.SelectedTime = new TimeSpan(tn.CountdownTime.Hours,tn.CountdownTime.Minutes,tn.CountdownTime.Seconds);
        }
    }
    void CountdownTimeModeTp_SelectedTimeChanged(object? sender,TimePickerSelectedValueChangedEventArgs e) {
        if (sender is TimePicker 
            {                                   
                DataContext: TimeNode tn,   
                SelectedTime: not null          
            } tp) {
            Console.WriteLine((tn.CountdownTime.Days, tp.SelectedTime.Value.Hours, tp.SelectedTime.Value.Minutes, tp.SelectedTime.Value.Seconds));
            tn.CountdownTime = new TimeSpan(tn.CountdownTime.Days, tp.SelectedTime.Value.Hours, tp.SelectedTime.Value.Minutes, tp.SelectedTime.Value.Seconds);
        }
    }
    void NumericUpDown_OnValueChanged(object? sender,NumericUpDownValueChangedEventArgs e) {
        if (sender is NumericUpDown 
            {                                   
                DataContext: TimeNode tn,   
                Value: not null      
            } numericUpDown) {
            //Console.WriteLine((tn.CountdownTime.Days, tn.SelectedTime.Value.Hours, tp.SelectedTime.Value.Minutes, tp.SelectedTime.Value.Seconds));
            tn.CountdownTime = new TimeSpan((int)numericUpDown.Value, tn.CountdownTime.Hours, tn.CountdownTime.Minutes, tn.CountdownTime.Seconds);
        }
    }
}