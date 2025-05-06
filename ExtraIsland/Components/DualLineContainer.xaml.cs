using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Components;

[ContainerComponent]
[ComponentInfo("C911D792-108F-49C6-84CC-0146AB8C86C1", "双行容器", PackIconKind.Fishbowl, "将多个组件组合到一个组件中显示")]
public partial class DualLineContainer {
    public DualLineContainer() {
        InitializeComponent();
    }

    void DualLineContainer_OnLoaded(object sender,RoutedEventArgs e) {
        Settings.Children.CollectionChanged += UpdateContent;
        UpItemsControl.ItemsSource = GetHalfComponents(Settings.Children);
        DownItemsControl.ItemsSource = GetHalfComponents(Settings.Children,true);
    }
    
    void UpdateContent(object? sender,NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
        UpItemsControl.ItemsSource = GetHalfComponents(Settings.Children);
        DownItemsControl.ItemsSource = GetHalfComponents(Settings.Children, true);
    }

    static ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings> 
        GetHalfComponents(ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings> set, bool isLater = false) {
        ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings> buffer = [];
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