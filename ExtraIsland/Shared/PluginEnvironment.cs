using System.IO;

namespace ExtraIsland.Shared;

/// <summary>
/// 插件运行环境信息,承载插件配置文件夹等宿主提供的数据
/// </summary>
public class PluginEnvironment {
    public PluginEnvironment(string configFolder) {
        ConfigFolder = configFolder;
    }

    public string ConfigFolder { get; }

    public string GetPath(string relativePath) {
        return Path.Combine(ConfigFolder,relativePath);
    }
}
