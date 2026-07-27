using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public class RhesisConfig : ObservableObject {
    Dictionary<string,RhesisProviderConfig> _providerSettings =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string,RhesisProviderConfig> ProviderSettings {
        get => _providerSettings;
        set => SetProperty(
            ref _providerSettings,
            value == null
                ? new Dictionary<string,RhesisProviderConfig>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string,RhesisProviderConfig>(value,StringComparer.OrdinalIgnoreCase));
    }

    public string IgnoreListString { get; set; } = string.Empty;

    public DateTime LastUpdate { get; set; } = DateTime.Today;

    public int LengthLimitation { get; set; }

    public TimeSpan UpdateTimeGap { get; set; } = TimeSpan.FromSeconds(30);

    [JsonIgnore]
    public double UpdateTimeGapSeconds {
        get => UpdateTimeGap.TotalSeconds;
        set => UpdateTimeGap = TimeSpan.FromSeconds(value);
    }
    
    public bool IsAnimationEnabled { get; set; } = true;
    
    public bool IsSwapAnimationEnabled { get; set; }

    public bool IsAuthorShowEnabled { get; set; }
    public bool IsTitleShowEnabled { get; set; }

    int _attributesShowingInterval = 3;
    public int AttributesShowingInterval {
        get => _attributesShowingInterval;
        set {
            if(_attributesShowingInterval == value) return;
            _attributesShowingInterval = value;
            OnPropertyChanged();
        }
    }

    AttributesDisplayRule _attributesRule = AttributesDisplayRule.Sametime;
    public AttributesDisplayRule AttributesRule {
        get => _attributesRule;
        set {
            if (value == _attributesRule) return;
            _attributesRule = value;
            OnPropertyChanged();
        }
    }

    public enum AttributesDisplayRule {
        [Description("同时展示")]
        Sametime,
        [Description("分开展示")]
        Separate
    }

    // 兼容旧版单选来源配置, 在v2移除
    [JsonPropertyName("DataSource")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LegacyRhesisDataSource? LegacyDataSource { get; set; }

    [JsonPropertyName("HitokotoProp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? LegacyHitokotoProp { get; set; }

    [JsonPropertyName("SainticProp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? LegacySainticProp { get; set; }

    public void EnsureProviderSettings(IEnumerable<IRhesisProvider> providers) {
        IRhesisProvider[] providerArray = providers.ToArray();
        bool shouldMigrateLegacySettings = ProviderSettings.Count == 0;

        foreach (IRhesisProvider provider in providerArray) {
            if (ProviderSettings.ContainsKey(provider.Id)) continue;
            ProviderSettings[provider.Id] = new RhesisProviderConfig {
                IsEnabled = provider.IsEnabledByDefault,
                Weight = provider.DefaultWeight
            };
        }

        if (!shouldMigrateLegacySettings || providerArray.Length == 0) return;
        ApplyLegacySourceSelection();

        if (ProviderSettings.TryGetValue(HitokotoRhesisProvider.ProviderId,out RhesisProviderConfig? hitokoto)
            && !string.IsNullOrWhiteSpace(LegacyHitokotoProp)) {
            hitokoto.SetOption(HitokotoRhesisProvider.QueryOption,LegacyHitokotoProp);
        }
        if (ProviderSettings.TryGetValue(SainticRhesisProvider.ProviderId,out RhesisProviderConfig? saintic)
            && !string.IsNullOrWhiteSpace(LegacySainticProp)) {
            saintic.SetOption(SainticRhesisProvider.PathOption,LegacySainticProp);
        }

        LegacyDataSource = null;
        LegacyHitokotoProp = null;
        LegacySainticProp = null;
        OnPropertyChanged(nameof(ProviderSettings));
    }

    void ApplyLegacySourceSelection() {
        if (LegacyDataSource is null) return;

        foreach (RhesisProviderConfig settings in ProviderSettings.Values) {
            settings.IsEnabled = false;
        }

        switch (LegacyDataSource.Value) {
            case LegacyRhesisDataSource.SaintJinrishici:
                EnableProvider(SainticRhesisProvider.ProviderId,1);
                EnableProvider(JinrishiciRhesisProvider.ProviderId,1);
                break;
            case LegacyRhesisDataSource.All:
                EnableProvider(SainticRhesisProvider.ProviderId,2);
                EnableProvider(JinrishiciRhesisProvider.ProviderId,1);
                EnableProvider(HitokotoRhesisProvider.ProviderId,1);
                break;
            case LegacyRhesisDataSource.Saint:
                EnableProvider(SainticRhesisProvider.ProviderId,1);
                break;
            case LegacyRhesisDataSource.Jinrishici:
                EnableProvider(JinrishiciRhesisProvider.ProviderId,1);
                break;
            case LegacyRhesisDataSource.Hitokoto:
                EnableProvider(HitokotoRhesisProvider.ProviderId,1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void EnableProvider(string providerId,int weight) {
        if (!ProviderSettings.TryGetValue(providerId,out RhesisProviderConfig? settings)) return;
        settings.IsEnabled = true;
        settings.Weight = weight;
    }
}

public enum LegacyRhesisDataSource {
    SaintJinrishici = -1,
    All = 0,
    Saint = 1,
    Jinrishici = 2,
    Hitokoto = 3
}
