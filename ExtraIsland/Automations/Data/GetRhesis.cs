using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Data;

public class GetRhesis {
    public int HitokotoWeight { get; set; }
    public string HitokotoQuery { get; set; } = string.Empty;
    
    public int JinrishiciWeight { get; set; }
    
    public int SainticWeight { get; set; }
    public string SainticPath { get; set; } = string.Empty;
    
    public int LengthLimitation { get; set; }
    public string IgnoreListString { get; set; } = string.Empty;

    static readonly RhesisHandler.Instance Rhesis = new RhesisHandler.Instance();

    public static Task<string> Getter(object? data) {
        return data is not GetRhesis config 
            ? Task.FromResult("???") : GetterAsync(config);
    }

    static async Task<string> GetterAsync(GetRhesis config) {
        Dictionary<string,RhesisProviderConfig> providerSettings = 
            new Dictionary<string,RhesisProviderConfig>(StringComparer.OrdinalIgnoreCase);
        foreach (IRhesisProvider provider in RhesisHandler.Providers) {
            providerSettings[provider.Id] = new RhesisProviderConfig {
                IsEnabled = provider.IsEnabledByDefault,
                Weight = provider.DefaultWeight
            };
        }
        Configure(providerSettings,
            HitokotoRhesisProvider.ProviderId,
            config.HitokotoWeight,
            HitokotoRhesisProvider.QueryOption,
            config.HitokotoQuery);
        Configure(providerSettings,
            JinrishiciRhesisProvider.ProviderId,
            config.JinrishiciWeight);
        Configure(providerSettings,
            SainticRhesisProvider.ProviderId,
            config.SainticWeight,
            SainticRhesisProvider.PathOption,
            config.SainticPath);

        RhesisData last = new RhesisData();
        for (int i = 0; i < 3; i++) {
            last = await Rhesis.GetAsync(providerSettings,config.LengthLimitation);
            if (!IsIgnored(config.IgnoreListString,last.Content)) return last.Content;
        }
        return last.Content;
    }

    static void Configure(Dictionary<string,RhesisProviderConfig> settings,
        string providerId, int weight,
        string? optionKey = null, string? optionValue = null) {
        if (!settings.TryGetValue(providerId,out RhesisProviderConfig? providerConfig)) return;
        providerConfig.IsEnabled = weight != 0;
        providerConfig.Weight = weight;
        if (!string.IsNullOrEmpty(optionKey) && !string.IsNullOrEmpty(optionValue)) {
            providerConfig.SetOption(optionKey,optionValue);
        }
    }

    static bool IsIgnored(string ignoreList,string content) {
        return ignoreList.Split("\r\n")
            .Any(keyWord => keyWord != "" && content.Contains(keyWord));
    }
}
