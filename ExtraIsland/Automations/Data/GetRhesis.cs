using ExtraIsland.Shared;
using SuperAutoIsland.Interface.Metadata;
using SuperAutoIsland.Interface.Services;
using SuperAutoIsland.Interface.Services.Automations;

namespace ExtraIsland.Automations.Data;

public class GetRhesisBlock : DataBlockBase {
    public override string Id { get => "extraIsland.data.getRhesis"; }
    public override string Name { get => "获取名句"; }
    public override (string,string) Icon { get => ("获取名句","\uE3F4"); }
    public override string DataOutput { get => "String"; }
    public override Type SettingsType { get => typeof(GetRhesisBlock); }
    public int HitokotoWeight { get; set; }
    public string HitokotoQuery { get; set; } = string.Empty;
    public int JinrishiciWeight { get; set; }
    public int SainticWeight { get; set; }
    public string SainticPath { get; set; } = string.Empty;
    public int LengthLimitation { get; set; }
    public string IgnoreListString { get; set; } = string.Empty;
    static readonly RhesisHandler.Instance Rhesis = new RhesisHandler.Instance();

    public override void GetFields(FieldsRegister it) => it
        .AddField("Dummy",BasicFields.Dummy(""))
        .AddField("HitokotoDummy",BasicFields.Dummy("一言"))
        .AddField("HitokotoWeight",BasicFields.Number("├ 权重"))
        .AddField("HitokotoQuery",BasicFields.Text("╰ 附加查询参数"))
        .AddField("JinrishiciDummy",BasicFields.Dummy("今日诗词"))
        .AddField("JinrishiciWeight",BasicFields.Number("╰ 权重"))
        .AddField("SainticDummy",BasicFields.Dummy("诏预"))
        .AddField("SainticWeight",BasicFields.Number("├ 权重"))
        .AddField("SainticPath",BasicFields.Text("╰ 接口路径"))
        .AddField("LengthLimitation",BasicFields.Number("字数限制"))
        .AddField("IgnoreListString",BasicFields.Text("排除列表(回车分隔)"));

    public override async Task<object> Handler(object? data) {
        if (data is not GetRhesisBlock config)
            return "???";
        return await GetterAsync(config);
    }

    static async Task<object> GetterAsync(GetRhesisBlock config) {
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
        string providerId,
        int weight,
        string? optionKey = null,
        string? optionValue = null) {
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