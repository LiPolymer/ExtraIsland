using System.Windows;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ComponentInfo(
    "795240DC-9457-D810-5806-7011BC7B28DA",
    "行动按钮",
    PackIconKind.Button,
    "点击执行特定行动"
)]
public partial class ActionButton {
    public ActionButton(IActionService service) {
        _service = service;
        InitializeComponent();
    }
    readonly IActionService _service;
    void RunActionSet(object sender,RoutedEventArgs e) {
        _service.Invoke(Settings.ActionSet);
    }
}