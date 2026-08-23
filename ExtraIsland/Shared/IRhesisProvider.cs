using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Shared;

public class RhesisData {
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
}

/// <summary>
/// Provides rhesis content from a single source.
/// </summary>
public interface IRhesisProvider {
    string Id { get; }
    string DisplayName { get; }
    string Description { get; }
    bool IsEnabledByDefault { get; }
    int DefaultWeight { get; }

    Task<RhesisData> FetchAsync(
        RhesisProviderConfig config,
        int lengthLimitation,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Optionally lets a provider supply its own settings content.
/// </summary>
public interface IRhesisProviderSettingsFactory {
    Control CreateSettingsControl(RhesisProviderConfig config);
}

public class RhesisProviderConfig : ObservableObject {
    bool _isEnabled;
    int _weight = 1;
    Dictionary<string,string> _options = [];

    public bool IsEnabled {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled,value);
    }

    public int Weight {
        get => _weight;
        set => SetProperty(ref _weight,Math.Max(0,value));
    }

    public Dictionary<string,string> Options {
        get => _options;
        set => SetProperty(ref _options,value ?? []);
    }

    public string GetOption(string key,string defaultValue = "") {
        return Options.TryGetValue(key,out string? value) ? value : defaultValue;
    }

    public void SetOption(string key,string? value) {
        if (string.IsNullOrEmpty(value)) {
            Options.Remove(key);
        } else {
            Options[key] = value;
        }
        OnPropertyChanged(nameof(Options));
    }
}
