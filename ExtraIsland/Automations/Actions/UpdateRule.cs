using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;

namespace ExtraIsland.Automations.Actions;

[ActionInfo("extraIsland.action.updateRule", "更新规则集", "\uE06D")]
public class UpdateRuleAction : ActionBase {
    readonly IRulesetService _rulesetService;

    public UpdateRuleAction(IRulesetService rulesetService) {
        _rulesetService = rulesetService;
    }

    protected override Task OnInvoke() {
        base.OnInvoke();
        Dispatcher.UIThread.Invoke(() => {
            _rulesetService.NotifyStatusChanged();
        });
        return Task.CompletedTask;
    }
}
