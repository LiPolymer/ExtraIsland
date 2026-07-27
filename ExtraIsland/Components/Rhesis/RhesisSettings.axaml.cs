using System.Collections.ObjectModel;
using Avalonia.Controls;
using ClassIsland.Core.Abstractions.Controls;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

public partial class RhesisSettings : ComponentBase<RhesisConfig> {
    public RhesisSettings() {
        InitializeComponent();
        AttachedToVisualTree += (_,_) => RefreshProviderItems();
    }

    public ObservableCollection<RhesisProviderSettingsItem> ProviderItems { get; } = [];

    public List<RhesisConfig.AttributesDisplayRule> AttributesRules { get; } = [
        RhesisConfig.AttributesDisplayRule.Sametime,
        RhesisConfig.AttributesDisplayRule.Separate
    ];

    void RefreshProviderItems() {
        Settings.EnsureProviderSettings(RhesisHandler.Providers);
        ProviderItems.Clear();
        foreach (IRhesisProvider provider in RhesisHandler.Providers) {
            ProviderItems.Add(new RhesisProviderSettingsItem(
                provider,
                Settings.ProviderSettings[provider.Id]));
        }
    }
}

public sealed class RhesisProviderSettingsItem(IRhesisProvider provider,RhesisProviderConfig configuration) {
    public string Id { get; } = provider.Id;
    public string DisplayName { get; } = provider.DisplayName;
    public string Description { get; } = provider.Description;
    public RhesisProviderConfig Configuration { get; } = configuration;
    public Control? SettingsControl { get; } = (provider as IRhesisProviderSettingsFactory)?.CreateSettingsControl(configuration);
    public bool HasSettings { get => SettingsControl != null; }
}
