using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Automations.Rules;

public partial class LaterThan {
    public LaterThan() {
        InitializeComponent();
    }
    
    public static bool Rule(object? rawConfig) {
        LaterThanConfig config = (LaterThanConfig)rawConfig!;
        TimeSpan current = DateTime.Now.TimeOfDay;
        return current.CompareTo(config.TargetTime.TimeOfDay) switch {
            < 0 => false,
            >= 0 => true
        };
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class LaterThanConfig : ObservableRecipient {
    public DateTime TargetTime { get; set; } = DateTime.Now;
}