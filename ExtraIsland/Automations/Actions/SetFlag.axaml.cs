using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Actions;

public partial class SetFlag: ActionSettingsControlBase<SetFlagConfig> {
    public SetFlag() {
        InitializeComponent();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public partial class SetFlagConfig : ObservableRecipient {
    [ObservableProperty]
    string _targetFlag = "";
    [ObservableProperty]
    string _flagContent = "";

    [ObservableProperty]
    bool _isPersisted;
    [ObservableProperty]
    bool _willNotifyUpdate = true;
}

/// <summary>
/// 行动 v3 提供方: 设/恢复标志
/// </summary>
[ActionInfo("extraIsland.action.setFlag", "设标志", "\uE844")]
public class SetFlagAction : ActionBase<SetFlagConfig> {
    readonly IFlagService _flagService;

    public SetFlagAction(IFlagService flagService) {
        _flagService = flagService;
    }

    protected override Task OnInvoke() {
        base.OnInvoke();
        SetFlagConfig settings = Settings;
        _flagService.SetValue(settings.TargetFlag,settings.FlagContent,settings.IsPersisted);
        Dispatcher.UIThread.Invoke(() => _flagService.NotifyStatusChanged());
        return Task.CompletedTask;
    }
    
    protected override Task OnRevert() {
        base.OnRevert();
        SetFlagConfig settings = Settings;
        _flagService.RemoveValue(settings.TargetFlag,settings.IsPersisted);
        if (Settings.WillNotifyUpdate) Dispatcher.UIThread.Invoke(() => _flagService.NotifyStatusChanged());
        return Task.CompletedTask;
    }
}