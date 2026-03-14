using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
            tp.SelectedTime = new TimeSpan(tn.CountdownTime.Hours,tn.CountdownTime.Minutes,tn.CountdownTime.Seconds);
        }
    }
    
    DispatcherTimer? _waitTimer;
    public void OnTpTimeChanged() {
        _waitTimer?.Start();
        _waitTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(3),
            DispatcherPriority.Background,
            (_, __) => {
                _waitTimer?.Stop();
                Times.SortAll();
            });
    }

    bool _isProcessing = false; 
    async void CountdownTimeModeTp_SelectedTimeChanged(object? sender,TimePickerSelectedValueChangedEventArgs e) {
        if(_isProcessing) return;
        if (sender is TimePicker 
            {                                   
                DataContext: TimeNode tn,   
                SelectedTime: not null          
            } tp) {
            TimeSpan newTime = new TimeSpan(tn.CountdownTime.Days, tp.SelectedTime.Value.Hours, tp.SelectedTime.Value.Minutes, tp.SelectedTime.Value.Seconds);
            if (newTime == tn.CountdownTime) return;
            _isProcessing =  true;
            try {
                tn.CountdownTime = newTime;
                OnTpTimeChanged();
            }
            finally {
                await Task.Delay(200);
                _isProcessing = false;
            }
        }
    }
    async void NumericUpDown_OnValueChanged(object? sender,NumericUpDownValueChangedEventArgs e) {
        if (sender is NumericUpDown 
            {                                   
                DataContext: TimeNode tn,   
                Value: not null      
            } numericUpDown) {
            TimeSpan newTime = new TimeSpan((int)numericUpDown.Value, tn.CountdownTime.Hours, tn.CountdownTime.Minutes, tn.CountdownTime.Seconds);
            if (newTime == tn.CountdownTime) return;
            _isProcessing =  true;
            try {
                tn.CountdownTime = newTime;
                OnTpTimeChanged();
            }
            finally {
                await Task.Delay(200);
                _isProcessing = false;
            }
        }
    }
}