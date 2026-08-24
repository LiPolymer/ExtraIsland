using ExtraIsland.Automations.Rules;
using ExtraIsland.Shared;
using SuperAutoIsland.Interface.Metadata;
using SuperAutoIsland.Interface.Services;
using SuperAutoIsland.Interface.Services.Automations;

namespace ExtraIsland.Automations.Data;

public class GetFlagBlock : DataBlockBase {
    public override string Id { get => "extraIsland.data.getFlag"; }
    public override string Name { get => "读标志"; }
    public override (string,string) Icon { get => ("读标志","\uE844"); }
    public override string DataOutput { get => "String"; }
    public override Type SettingsType { get => typeof(GetFlagBlock); }
    public string TargetFlag { get; set; } = string.Empty;

    public override void GetFields(FieldsRegister it) => it
        .AddField("TargetFlag",BasicFields.Text("ID"));

    public override Task<object> Handler(object? data) {
        if (data is not GetFlagBlock config) return Task.FromResult<object>("???");
        Dictionary<string,string> merged = GlobalConstants.Handlers.PersistedFlagHandler?.FlagsTable != null
            ? new Dictionary<string,string>(GlobalConstants.Handlers.PersistedFlagHandler.FlagsTable)
            : [];
        foreach (KeyValuePair<string,string> kv in Flag.Flags)
            merged[kv.Key] = kv.Value; // 内存标志覆盖持久化标志
        return Task.FromResult<object>(merged.GetValueOrDefault(config.TargetFlag,"[未设置值]"));
    }
}