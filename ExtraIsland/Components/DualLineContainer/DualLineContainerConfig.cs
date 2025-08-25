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

    ObservableCollection<ComponentSettings> _children = [];
    public ObservableCollection<ComponentSettings> Children {
        get => _children;
        set {
            if (Equals(value,_children)) return;
            _children = value;
            OnPropertyChanged();
            ContainerContentChanged?.Invoke();
            value.CollectionChanged += (_,_) => ContainerContentChanged?.Invoke();
        }
    }

    [ObservableProperty]
    ObservableCollection<ComponentSettings> _upChildren = [];

    [ObservableProperty]
    ObservableCollection<ComponentSettings> _downChildren = [];
}