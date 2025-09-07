using ClassIsland.Shared.Helpers;
using ExtraIsland.Shared;

namespace ExtraIsland.ConfigHandlers;

public class PersistedFlagHandler: ConfigBase {
    protected override string Path { get => "Persisted/Flags.json"; }
    
    public Dictionary<string,string> FlagsTable { get; set; } = [];

    public override void Save() {
        // 临时方案
        ConfigureFileHelper.SaveConfig(System.IO.Path.Combine(GlobalConstants.PluginConfigFolder!,Path),this);
    }
}