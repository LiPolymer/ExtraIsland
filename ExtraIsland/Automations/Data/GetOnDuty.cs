using ExtraIsland.Shared;
using SuperAutoIsland.Interface.Services.Automations;

namespace ExtraIsland.Automations.Data;

public class GetOnDutyBlock : DataBlockBase {
    public override string Id { get => "extraIsland.data.getOnDuty"; }
    public override string Name { get => "获取当前值日生"; }
    public override (string,string) Icon { get => ("获取当前值日生","\uECDB"); }
    public override string DataOutput { get => "String"; }
    public override Type SettingsType { get => typeof(GetOnDutyBlock); }

    public override Task<object> Handler(object? data)
        => Task.FromResult<object>(GlobalConstants.Handlers.OnDuty?.PeoplesOnDutyString ?? "???");
}