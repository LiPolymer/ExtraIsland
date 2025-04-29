using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;

namespace ExtraIsland.Components;

public partial class ActionButtonSettings {
    public ActionButtonSettings(IActionService actionService) {
        _actionService = actionService;
        InitializeComponent();
    }

    readonly IActionService _actionService;

    private void RunActionSet(object sender,System.Windows.RoutedEventArgs e) {
        _actionService.Invoke(Settings.ActionSet);
    }
}