using System.ComponentModel;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

[ComponentInfo("B977ECCC-1A59-4C71-A4EB-67780E16E926", "值日生", "\uECDB", "显示值日生姓名，每日轮换(ExtraIsland)" )]
public partial class OnDuty : ComponentBase<OnDutyConfig> {
    OnDutyPersistedConfigHandler PersistedSettings { get; }
    
    public OnDuty(OnDutyPersistedConfigHandler onDutyHandler) {
        PersistedSettings = onDutyHandler;
        InitializeComponent();
    }
    
    void OnOnDutyUpdated() {
        Dispatcher.UIThread.Invoke(() => {
            if (!Settings.IsCompactModeEnabled) {
                NameLabel.Content = PersistedSettings.PeoplesOnDutyString;
            } else {
                if (PersistedSettings.Data.DutyState == OnDutyPersistedConfigData.DutyStateData.InOut) {
                    DualLabelUp.Content = "内 " + PersistedSettings.PeoplesOnDuty[0].Name;
                    DualLabelDown.Content = "外 " + PersistedSettings.PeoplesOnDuty[1].Name;
                } else {
                    List<string> upc = [];
                    List<string> dnc = [];
                    int i = 0;
                    if (EiUtils.IsOdd(PersistedSettings.PeoplesOnDuty.Count)) {
                        i++;
                        upc.Add("值日");
                    }
                    foreach (OnDutyPersistedConfigData.PeopleItem pit in PersistedSettings.PeoplesOnDuty) {
                        i++;
                        if (EiUtils.IsOdd(i)) upc.Add(pit.Name);
                        else dnc.Add(pit.Name);
                    }
                    DualLabelUp.Content = string.Join(" ", upc);
                    DualLabelDown.Content = string.Join(" ", dnc);
                }
            }
        });
    }
    
    void OnOnDutyUpdated(object? sender, PropertyChangedEventArgs e) {
        OnOnDutyUpdated();
    }
    
    void OnDuty_OnUnloaded(object sender, RoutedEventArgs e) {
        PersistedSettings.OnDutyUpdated -= OnOnDutyUpdated;
        Settings.PropertyChanged -= OnOnDutyUpdated;
    }

    void OnDuty_OnLoaded(object sender, RoutedEventArgs e) {
        OnOnDutyUpdated();
        Settings.PropertyChanged += OnOnDutyUpdated;
        PersistedSettings.OnDutyUpdated += OnOnDutyUpdated;
    }
}
