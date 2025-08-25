using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Data;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using ClassIsland.Core.Models.Components;
using ExtraIsland.Shared.Converters;

namespace ExtraIsland.Components;

[ContainerComponent]
[ComponentInfo("1FA88C26-6E17-4CF5-9BB4-771C7527FD1B", "双行容器", "😰", "将多个组件组合到两小行中显示")]
public partial class DualLineContainer: ComponentBase<DualLineContainerConfig> {
    public DualLineContainer() {
        InitializeComponent();
    }

    void DualLineContainer_OnLoaded(object? sender,RoutedEventArgs routedEventArgs) {
        Settings.ContainerContentChanged += UpdateContent;
    }
    
    void UpdateContent() {
        Console.WriteLine("Updated");
        Settings.UpChildren = GetHalfComponents(Settings.Children);
        Settings.DownChildren = GetHalfComponents(Settings.Children,true);
    }

    static ObservableCollection<ComponentSettings> GetHalfComponents(ObservableCollection<ComponentSettings> set, bool isLater = false) {
        ObservableCollection<ComponentSettings> buffer = [];
        int t;
        if (set.Count == 0) {
            return buffer;
        }
        if (EiUtils.IsOdd(set.Count)) {
            t = (set.Count + 1)/2;
        } else {
            t = set.Count/2;
        }
        for (int i = 0; i <= t-1; i++) {
            int ai = isLater ? (i+t) : i;
            if (set.Count < ai + 1) continue;
            buffer.Add(set[ai]);
        }
        return buffer;
    }
}