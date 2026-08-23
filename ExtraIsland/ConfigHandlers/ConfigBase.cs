using System.ComponentModel;
using System.IO;
using ClassIsland.Shared.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.ConfigHandlers;

public abstract class ConfigBase: ObservableObject {
    internal PluginEnvironment Environment { get; set; } = null!;

    protected ConfigBase() {
        // ReSharper disable once VirtualMemberCallInConstructor
        OnInitializing();
        PropertyChanged += Save;
    }

    public virtual void OnInitializing() {}

    protected abstract string Path { get; }

    protected string GetConfigPath() {
        return Environment.GetPath(Path);
    }

    /// <summary>
    /// 加载配置文件:文件不存在时自动创建,返回已绑定环境的实例
    /// </summary>
    public static T Load<T>(PluginEnvironment environment) where T : ConfigBase,new() {
        T instance = new T();
        instance.Environment = environment;
        string finalPath = instance.GetConfigPath();
        if (!File.Exists(finalPath)) {
            ConfigureFileHelper.SaveConfig(finalPath,instance);
            return instance;
        }
        T loaded = ConfigureFileHelper.LoadConfig<T>(finalPath);
        loaded.Environment = environment;
        return loaded;
    }

    void Save(object? sender,PropertyChangedEventArgs e) {
        Save();
    }

    public virtual void Save() {
        ConfigureFileHelper.SaveConfig(GetConfigPath(),this);
    }
}
