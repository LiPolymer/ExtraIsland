using System.Collections.ObjectModel;
using ClassIsland.Core.Abstractions.Models;
using ClassIsland.Core.Models.Components;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class DualLineContainerConfig : ObservableObject, IComponentContainerSettings {
    public DualLineContainerConfig() {
        Children.CollectionChanged += (_,_) => ContainerContentChanged?.Invoke();
    }
    
    public event Action? ContainerContentChanged;

    public ObservableCollection<ComponentSettings> Children { get;
        set {
            if (Equals(value,field)) return;
            field = value;
            OnPropertyChanged();
            ContainerContentChanged?.Invoke();
            value.CollectionChanged += (_,_) => ContainerContentChanged?.Invoke();
        }
    } = [];

    [ObservableProperty]
    ObservableCollection<ComponentSettings> _upChildren = [];

    [ObservableProperty]
    ObservableCollection<ComponentSettings> _downChildren = [];

    [ObservableProperty]
    double _upScale = 0.6;

    [ObservableProperty]
    double _downScale = 0.7;

    [ObservableProperty]
    double _rowSpacing = 4;
}