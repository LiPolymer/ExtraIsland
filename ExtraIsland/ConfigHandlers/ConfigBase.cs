using System.ComponentModel;
using System.Text.Json.Serialization;
using ClassIsland.Shared.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using ExtraIsland.Shared;

namespace ExtraIsland.ConfigHandlers;

public abstract class ConfigBase: ObservableObject {
    public ConfigBase() {
        // ReSharper disable once VirtualMemberCallInConstructor
        OnInitializing();
        PropertyChanged += Save;
    }

    public virtual void OnInitializing() {}
    
    protected abstract string Path { get; }

    public static T Load<T>() where T : ConfigBase,new() {
        T instance = new T();
        string finalPath = System.IO.Path.Combine(GlobalConstants.PluginConfigFolder!,instance.Path);
        if (!File.Exists(finalPath)) {
            ConfigureFileHelper.SaveConfig(finalPath,instance);
        }
        return ConfigureFileHelper.LoadConfig<T>(finalPath);
    }
    
    void Save(object? sender,PropertyChangedEventArgs e) {
        Save();
    }
    
    public virtual void Save() {
        // todo:行为异常, 目前只保存ConfigBase而不是子类
        ConfigureFileHelper.SaveConfig(System.IO.Path.Combine(GlobalConstants.PluginConfigFolder!,Path),this);
    }
}