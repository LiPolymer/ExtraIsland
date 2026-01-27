namespace ExtraIsland.Components;

public interface ITimeUp {
    public event EventHandler? OnTimeUp;
    public void RaiseOnTimeUp(object? sender, EventArgs m);
}