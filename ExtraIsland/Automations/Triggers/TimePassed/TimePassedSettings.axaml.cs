﻿using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Automations.Triggers;

public partial class TimePassedSettings: TriggerSettingsControlBase<TimePassedConfig> {
    public TimePassedSettings() {
        InitializeComponent();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class TimePassedConfig : ObservableRecipient {

    int _timeGapSeconds = 120;

    public int TimeGapSeconds {
        get => _timeGapSeconds;
        set {
            if (value == _timeGapSeconds) return;
            _timeGapSeconds = value;
            OnPropertyChanged();
        }
    }
    
    [JsonIgnore]
    public TimeSpan TimeGap {
        get => TimeSpan.FromSeconds(_timeGapSeconds);
    }

    DateTime _lastTriggered = DateTime.Now;
    public DateTime LastTriggered {
        get => _lastTriggered;
        set {
            if (value == _lastTriggered) return;
            _lastTriggered = value;
            OnPropertyChanged();
        }
    }
}