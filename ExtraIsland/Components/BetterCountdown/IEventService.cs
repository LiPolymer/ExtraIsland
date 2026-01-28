namespace ExtraIsland.Components;

public interface IEventService {
    public event EventHandler? OnTimeUp;
    public event EventHandler? OnTargetTimeChanged;
    public event EventHandler? OnDetachedFromVisualTreeEventE;
    public event EventHandler? OnAttachedToVisualTreeE;
    
    public void RaiseOnTimeUp(object? sender, EventArgs m);
    public void RaiseOnTargetTimeChanged(object? sender, EventArgs m);
    public void RaiseOnDetachedFromVisualTreeEventE();
    public void RaiseOnAttachedToVisualTreeE();
}