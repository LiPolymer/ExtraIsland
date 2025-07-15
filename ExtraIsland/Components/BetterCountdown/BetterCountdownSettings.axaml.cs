using ClassIsland.Core.Abstractions.Controls;

namespace ExtraIsland.Components;

public partial class BetterCountdownSettings : ComponentBase<BetterCountdownConfig> {
    public BetterCountdownSettings() {
        InitializeComponent();
    }
    
    public List<CountdownAccuracy> CountdownAccuracies { get; } = [
        CountdownAccuracy.Day,
        CountdownAccuracy.Hour,
        CountdownAccuracy.Minute,
        CountdownAccuracy.Second
    ]; 
}