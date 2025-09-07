using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Actions;

public partial class SetFlag: ActionSettingsControlBase<SetFlagConfig> {
    public SetFlag() {
        InitializeComponent();
    }
    
    public static void Action(object? rawConfig, string _) {
        SetFlagConfig config = (SetFlagConfig)rawConfig!;
        if (config.IsPersisted) {
            WriteDict(GlobalConstants.Handlers.PersistedFlagHandler!.FlagsTable,config.TargetFlag,config.FlagContent);
            GlobalConstants.Handlers.PersistedFlagHandler.Save();
        } else {
            WriteDict(Flag.Flags,config.TargetFlag,config.FlagContent);   
        }
    }

    static void WriteDict(Dictionary<string,string> dict,string key,string value) {
        if (dict.TryGetValue(key, out string _)) { 
            dict[key] = value;
        } else {
            dict.Add(key,value);
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class SetFlagConfig : ObservableRecipient {
    public string TargetFlag { get; set; } = "";
    public string FlagContent { get; set; } = "";

    public bool IsPersisted { get; set; } = false;
}