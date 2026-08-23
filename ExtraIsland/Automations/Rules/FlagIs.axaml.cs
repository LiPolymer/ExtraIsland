using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;

public partial class FlagIs: RuleSettingsControlBase<FlagIsConfig> {
    public FlagIs() {
        InitializeComponent();
    }
    
    public static bool Rule(object? rawConfig,IFlagService flagService) {
        FlagIsConfig config = (FlagIsConfig)rawConfig!;
        return flagService.TryGetValue(config.TargetFlag,out string? flagContent)
               && flagContent == config.FlagContent;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class FlagIsConfig : ObservableRecipient {
    public string TargetFlag { get; set; } = "";
    public string FlagContent { get; set; } = "";
}
