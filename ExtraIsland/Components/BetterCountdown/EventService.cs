namespace ExtraIsland.Components;

public class EventService : IEventService{
    public event EventHandler? OnTimeUp;
    public event EventHandler? OnTargetTimeChanged;
    public event EventHandler? OnDetachedFromVisualTreeEventE;
    public void RaiseOnTimeUp(object? sender, EventArgs m) => OnTimeUp?.Invoke(sender, m);
    public void RaiseOnTargetTimeChanged(object? sender, EventArgs m) => OnTargetTimeChanged?.Invoke(sender, m);
    public void RaiseOnDetachedFromVisualTreeEventE() => OnDetachedFromVisualTreeEventE?.Invoke(this, EventArgs.Empty);
}