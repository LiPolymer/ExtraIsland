using ExtraIsland.Shared;

namespace ExtraIsland.ConfigHandlers;

public class PersistedFlagHandler: ConfigBase {
    protected override string Path { get => "Persisted/Flags.json"; }

    public Dictionary<string,string> FlagsTable { get; set; } = [];
}
