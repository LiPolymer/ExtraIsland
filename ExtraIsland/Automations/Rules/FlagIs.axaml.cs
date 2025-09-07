using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;

public static class Flag {
    public static readonly Dictionary<string,string> Flags = [];
}

public partial class FlagIs: RuleSettingsControlBase<FlagIsConfig> {
    public FlagIs() {
        InitializeComponent();
    }
    
    public static bool Rule(object? rawConfig) {
        FlagIsConfig config = (FlagIsConfig)rawConfig!;
        string? flagContent = null;
        if (Flag.Flags.Concat(GlobalConstants.Handlers.PersistedFlagHandler!.FlagsTable)
            .ToDictionary()
            .TryGetValue(config.TargetFlag,out string? flag)) flagContent = flag;
        return flagContent == config.FlagContent;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class FlagIsConfig : ObservableRecipient {
    public string TargetFlag { get; set; } = "";
    public string FlagContent { get; set; } = "";
}