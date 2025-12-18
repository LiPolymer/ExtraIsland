using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Actions;

public partial class SetFlag: ActionSettingsControlBase<SetFlagConfig> {
    public SetFlag() {
        InitializeComponent();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class SetFlagConfig : ObservableRecipient {
    public string TargetFlag { get; set; } = "";
    public string FlagContent { get; set; } = "";

    public bool IsPersisted { get; set; }
    public bool WillNotifyUpdate { get; set; } = true;
}

/// <summary>
/// 行动 v3 提供方: 设/恢复标志
/// </summary>
[ActionInfo("extraIsland.action.setFlag", "设标志", "\uE844")]
public class SetFlagAction : ActionBase<SetFlagConfig> {
    protected override Task OnInvoke() {
        base.OnInvoke();
        SetFlagConfig settings = Settings;
        if (settings.IsPersisted) {
            WriteDict(GlobalConstants.Handlers.PersistedFlagHandler!.FlagsTable, settings.TargetFlag, settings.FlagContent);
            GlobalConstants.Handlers.PersistedFlagHandler.Save();
        } else {
            WriteDict(Flag.Flags, settings.TargetFlag, settings.FlagContent);
        }
        Dispatcher.UIThread.Invoke(() => {
            GlobalConstants.HostInterfaces.RulesetService?.NotifyStatusChanged();
        });
        return Task.CompletedTask;
    }
    
    protected override Task OnRevert() {
        base.OnRevert();
        SetFlagConfig settings = Settings;
        if (settings.IsPersisted) {
            GlobalConstants.Handlers.PersistedFlagHandler!.FlagsTable.Remove(settings.TargetFlag);
            GlobalConstants.Handlers.PersistedFlagHandler.Save();
        } else {
            Flag.Flags.Remove(settings.TargetFlag);
        }
        if (Settings.WillNotifyUpdate) Dispatcher.UIThread.Invoke(() => {
            GlobalConstants.HostInterfaces.RulesetService?.NotifyStatusChanged();
        });
        return Task.CompletedTask;
    }

    static void WriteDict(Dictionary<string, string> dict, string key, string value) {
        if (dict.TryGetValue(key, out _)) dict[key] = value; else dict.Add(key, value);
    }
}