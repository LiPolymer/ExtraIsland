using ExtraIsland.Shared;
using SuperAutoIsland.Interface.Metadata;
using SuperAutoIsland.Interface.Services;
using SuperAutoIsland.Interface.Services.Automations;

namespace ExtraIsland.Automations.Data;

public class GetOnDutyBlock : DataBlockBase {
    public override string Id => "extraIsland.data.getOnDuty";
    public override string Name => "获取当前值日生";
    public override (string,string) Icon => ("获取当前值日生","\uECDB");
    public override string DataOutput => "String";
    public override Type SettingsType => typeof(GetOnDutyBlock);

    public override Task<object> Handler(object? data)
        => Task.FromResult<object>(GlobalConstants.Handlers.OnDuty?.PeoplesOnDutyString ?? "???");
}