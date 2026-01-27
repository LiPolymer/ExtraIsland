namespace ExtraIsland.Components;

public class TimeUp : ITimeUp {
    public event EventHandler? OnTimeUp;
    public void RaiseOnTimeUp(object? sender, EventArgs m) => OnTimeUp?.Invoke(sender, m);
}