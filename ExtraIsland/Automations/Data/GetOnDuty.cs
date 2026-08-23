using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Data;

public class GetOnDuty {
    public static Task<string> Getter(object? data) 
        => Task.FromResult(GlobalConstants.Handlers.OnDuty?.PeoplesOnDutyString ?? "???");
}