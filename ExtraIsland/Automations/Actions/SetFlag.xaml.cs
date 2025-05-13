using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Shared;
using MahApps.Metro.Controls;

namespace ExtraIsland.Automations.Actions;

public partial class SetFlag {
    public SetFlag() {
        InitializeComponent();
    }
    
    public static void Action(object? rawConfig, string _) {
        SetFlagConfig config = (SetFlagConfig)rawConfig!;
        if (Flag.Flags.TryGetValue(config.TargetFlag, out string _)) {
            Flag.Flags[config.TargetFlag] = config.FlagContent;
        } else {
            Flag.Flags.Add(config.TargetFlag, config.FlagContent);
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class SetFlagConfig : ObservableRecipient {
    public string TargetFlag { get; set; } = "";
    public string FlagContent { get; set; } = "";
}