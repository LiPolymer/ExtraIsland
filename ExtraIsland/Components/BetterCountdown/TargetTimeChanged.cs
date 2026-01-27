namespace ExtraIsland.Components;

public class TargetTimeChanged : ITargetTimeChanged {
    public event EventHandler? OnTargetTimeChanged;
    public void RaiseOnTargetTimeChanged(object? sender, EventArgs m) => OnTargetTimeChanged?.Invoke(sender, m);
}