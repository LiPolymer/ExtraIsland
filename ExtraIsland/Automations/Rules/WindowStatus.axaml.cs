using ClassIsland.Core.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;

public partial class WindowStatus : RuleSettingsControlBase<WindowStatusConfig> {
    public WindowStatus() {
        InitializeComponent();
    }

    public static bool Rule(object? rawConfig) {
        WindowStatusConfig config = (WindowStatusConfig)rawConfig!;
        return WindowStatusDetect.Check(config.WindowStatus);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class WindowStatusConfig : ObservableRecipient {
    public int WindowStatus { get; set; } = WindowStatusDetect.StatusMaximized;
}
