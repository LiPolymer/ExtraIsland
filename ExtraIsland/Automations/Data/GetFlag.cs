using ExtraIsland.Automations.Rules;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Data;

public class GetFlag {
    public string TargetFlag { get; set; } = string.Empty;
    
    public static Task<string> Getter(object? data) {
        if (data is not GetFlag config) {
            return Task.FromResult("???");
        }
        Dictionary<string,string> merged = GlobalConstants.Handlers.PersistedFlagHandler?.FlagsTable != null
            ? new Dictionary<string, string>(GlobalConstants.Handlers.PersistedFlagHandler.FlagsTable)
            : [];
        foreach (KeyValuePair<string,string> kv in Flag.Flags)
            merged[kv.Key] = kv.Value; // 内存标志覆盖持久化标志
        return Task.FromResult(merged.GetValueOrDefault(config.TargetFlag,"[未设置值]"));
    }
}