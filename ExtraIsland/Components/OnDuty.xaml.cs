﻿using System.Windows;
using System.Windows.Controls;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Plugin;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
    "B977ECCC-1A59-4C71-A4EB-67780E16E926",
    "(未完成)值日生",
    PackIconKind.Users,
    "显示值日生姓名,每日轮换"
)]
public partial class OnDuty : ComponentBase {
    public OnDuty() {
        Settings = GlobalConstants.ConfigHandlers.OnDuty!;
        InitializeComponent();
        OnOnDutyUpdated();
        Settings.OnDutyUpdated += OnOnDutyUpdated;
    }
    void OnOnDutyUpdated() {
        NameLabel.Content = Settings.PeopleOnDuty.Name;
    }
    OnDutyPersistedConfig Settings { get; }
    void OnDuty_OnUnloaded(object sender,RoutedEventArgs e) {
        Settings.OnDutyUpdated -= OnOnDutyUpdated;
    }
}