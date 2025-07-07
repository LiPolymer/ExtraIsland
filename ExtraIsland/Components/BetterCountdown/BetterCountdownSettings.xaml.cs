namespace ExtraIsland.Components;

public partial class BetterCountdownSettings {
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