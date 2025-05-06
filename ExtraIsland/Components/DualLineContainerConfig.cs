using System.Collections.ObjectModel;
using ClassIsland.Core.Abstractions.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class DualLineContainerConfig : ObservableObject, IComponentContainerSettings {
    [ObservableProperty]
    private ObservableCollection<ClassIsland.Core.Models.Components.ComponentSettings> _children = [];
}