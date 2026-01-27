namespace ExtraIsland.Components;

public interface IDetectDetachedFromVisualTree {
    public event EventHandler? OnDetachedFromVisualTreeEventE;
    public void RaiseOnDetachedFromVisualTreeEventE();
}