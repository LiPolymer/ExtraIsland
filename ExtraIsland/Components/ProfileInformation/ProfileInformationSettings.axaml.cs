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

    public List<FirstDayOfWeek> FirstDayOfWeekOptions { get; } = [
        FirstDayOfWeek.Sunday,
        FirstDayOfWeek.Monday,
        FirstDayOfWeek.Tuesday,
        FirstDayOfWeek.Wednesday,
        FirstDayOfWeek.Thursday,
        FirstDayOfWeek.Friday,
        FirstDayOfWeek.Saturday
    ];
}