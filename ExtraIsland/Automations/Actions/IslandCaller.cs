using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Commands;
using Avalonia.Threading;

namespace ExtraIsland.Automations.Actions;

/// <summary>
/// 行动 v3 提供方: 拉起 IslandCaller
/// </summary>
[ActionInfo("extraIsland.action.islandCaller", "拉起IslandCaller", "\uECB5")]
public class IslandCallerAction : ActionBase {
    protected override Task OnInvoke() {
        base.OnInvoke();
        Dispatcher.UIThread.Invoke(() => {
            UriNavigationCommands.UriNavigationCommand.Execute("classisland://plugins/IslandCaller/Run");
        });
        return Task.CompletedTask;
    }
}
