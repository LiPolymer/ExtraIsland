using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using ExtraIsland.Shared.Converters;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;

namespace ExtraIsland.Components;

[ContainerComponent]
[ComponentInfo("1FA88C26-6E17-4CF5-9BB4-771C7527FD1B", "双行容器", PackIconKind.ArrowCollapseVertical, "将多个组件组合到两小行中显示")]
public partial class DualLineContainer {
    public DualLineContainer() {
        InitializeComponent();
    }

    void DualLineContainer_OnLoaded(object sender,RoutedEventArgs e) {
        BindingOperations.SetBinding(
            RootGrid,
            WidthProperty,
            new Binding("Children[0].ActualWidth") {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Converter = new DoubleMultipleConverter(),
                ConverterParameter = 0.55 
            });
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