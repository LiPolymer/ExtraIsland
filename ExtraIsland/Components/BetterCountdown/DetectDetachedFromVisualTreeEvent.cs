namespace ExtraIsland.Components;

public class DetectDetachedFromVisualTreeEvent : IDetectDetachedFromVisualTree {
    public event EventHandler? OnDetachedFromVisualTreeEventE;
    public void RaiseOnDetachedFromVisualTreeEventE() => OnDetachedFromVisualTreeEventE?.Invoke(this, EventArgs.Empty);
    
}