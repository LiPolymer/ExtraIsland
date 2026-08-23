using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;

public static class Flag {
    public static readonly Dictionary<string,string?> Flags = [];
}

public partial class FlagIs: RuleSettingsControlBase<FlagIsConfig> {
    public FlagIs() {
        InitializeComponent();
    }
    
    public static bool Rule(object? rawConfig) {
        FlagIsConfig config = (FlagIsConfig)rawConfig!;
        Dictionary<string,string> merged = GlobalConstants.Handlers.PersistedFlagHandler?.FlagsTable != null
            ? new Dictionary<string, string>(GlobalConstants.Handlers.PersistedFlagHandler.FlagsTable)
            : [];
        foreach (KeyValuePair<string,string> kv in Flag.Flags)
            merged[kv.Key] = kv.Value; // 内存标志覆盖持久化标志

        return merged.TryGetValue(config.TargetFlag,out string? flagContent)
               && flagContent == config.FlagContent;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class FlagIsConfig : ObservableRecipient {
    public string TargetFlag { get; set; } = "";
    public string FlagContent { get; set; } = "";
}
