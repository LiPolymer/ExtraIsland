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
    
    public OnDuty() {
        PersistedSettings = GlobalConstants.Handlers.OnDuty!;
        InitializeComponent();
    }
    
    void OnOnDutyUpdated() {
        Dispatcher.UIThread.Invoke(() => {
            if (!Settings.IsCompactModeEnabled) {
                NameLabel.Content = PersistedSettings.PeoplesOnDutyString;
            } else {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (PersistedSettings.Data.DutyState == OnDutyPersistedConfigData.DutyStateData.Single) {
                    DualLabelUp.Content = "值日";
                    DualLabelDown.Content = PersistedSettings.PeoplesOnDutyString;
                } else if (PersistedSettings.Data.DutyState == OnDutyPersistedConfigData.DutyStateData.Double) {
                    DualLabelUp.Content = PersistedSettings.PeoplesOnDuty[0].Name;
                    DualLabelDown.Content = PersistedSettings.PeoplesOnDuty[1].Name;
                } else if (PersistedSettings.Data.DutyState == OnDutyPersistedConfigData.DutyStateData.InOut) {
                    DualLabelUp.Content = "内 " + PersistedSettings.PeoplesOnDuty[0].Name;
                    DualLabelDown.Content = "外 " + PersistedSettings.PeoplesOnDuty[1].Name;
                } else {
                    DualLabelUp.Content = PersistedSettings.PeoplesOnDuty[0].Name + " " + PersistedSettings.PeoplesOnDuty[1].Name;
                    DualLabelDown.Content = PersistedSettings.PeoplesOnDuty[2].Name + " " + PersistedSettings.PeoplesOnDuty[3].Name;
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
