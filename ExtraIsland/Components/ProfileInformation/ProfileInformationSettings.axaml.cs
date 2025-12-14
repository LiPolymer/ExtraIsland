using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ClassIsland.Core.Abstractions.Controls;

namespace ExtraIsland.Components;

public partial class ProfileInformationSettings : ComponentBase<ProfileInformationConfig> {
    public ProfileInformationSettings() {
        InitializeComponent();
    }

    public List<ProfileInformationType> InformationTypes { get; } = [
        ProfileInformationType.WeekOfSemester,
        ProfileInformationType.ParityOfWeek
    ];
}