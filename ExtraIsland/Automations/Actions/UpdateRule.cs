using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Actions;

[ActionInfo("extraIsland.action.updateRule", "更新规则集", "\uECB5")]
public class UpdateRuleAction : ActionBase {
    protected override Task OnInvoke() {
        base.OnInvoke();
        Dispatcher.UIThread.Invoke(() => {
            GlobalConstants.HostInterfaces.RulesetService?.NotifyStatusChanged();
        });
        return Task.CompletedTask;
    }
}