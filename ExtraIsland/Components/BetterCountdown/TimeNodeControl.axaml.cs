using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class TimeNodeControl : UserControl {
    public TimeNodeControl() {
        InitializeComponent();
    }
    
    //public BetterCountdownConfig Config {get; set;}
    public static readonly StyledProperty<TimeNodeObservableCollection> TimesProperty =
        AvaloniaProperty.Register<TimeNodeControl, TimeNodeObservableCollection>(nameof(Times));
    
    public TimeNodeObservableCollection Times {
        get => GetValue(TimesProperty);
        set {
            Console.WriteLine("Times属性被设置！");
            SetValue(TimesProperty,value);
        }
    }
    
    public void ButtonAddTime_Click(object? sender, RoutedEventArgs e) {
        if (Times is null) {
            Console.WriteLine("Times为空");
        }
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
            tp.SelectedTime = new TimeSpan(tn.CountdownTime.Hours,tn.CountdownTime.Minutes,tn.CountdownTime.Seconds);
        }
    }
    
    void CountdownTimeModeTp_SelectedTimeChanged(object? sender,TimePickerSelectedValueChangedEventArgs e) {
        if (sender is TimePicker 
            {                                   
                DataContext: TimeNode tn,   
                SelectedTime: not null          
            } tp) {
            TimeSpan newTime = new TimeSpan(tn.CountdownTime.Days, tp.SelectedTime.Value.Hours, tp.SelectedTime.Value.Minutes, tp.SelectedTime.Value.Seconds);
            if (newTime == tn.CountdownTime) return;
                tn.CountdownTime = newTime;
        }
    }
    void NumericUpDown_OnValueChanged(object? sender,NumericUpDownValueChangedEventArgs e) {
        if (sender is NumericUpDown 
            {                                   
                DataContext: TimeNode tn,   
                Value: not null      
            } numericUpDown) {
            TimeSpan newTime = new TimeSpan((int)numericUpDown.Value, tn.CountdownTime.Hours, tn.CountdownTime.Minutes, tn.CountdownTime.Seconds);
            if (newTime == tn.CountdownTime) return;
                tn.CountdownTime = newTime;
            
        }
    }
    void SaveButton_OnClick(object? sender,RoutedEventArgs e) {
        if(Times.Count == 0) return;
        Console.WriteLine("SaveButton调用！");
        Times.GetLatest();
    }

}