namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class OnDutyConfig {
    public event Action? PropertyChanged;

    bool _isCompactModeEnabled;
    public bool IsCompactModeEnabled {
        get => _isCompactModeEnabled;
        set {
            if (_isCompactModeEnabled == value) return;
            _isCompactModeEnabled = value;
            PropertyChanged?.Invoke();
        }
    }
}