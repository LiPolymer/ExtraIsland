namespace ExtraIsland.Components;

public interface ITargetTimeChanged {
    public event EventHandler? OnTargetTimeChanged;
    public void RaiseOnTargetTimeChanged(object? sender, EventArgs m);
}