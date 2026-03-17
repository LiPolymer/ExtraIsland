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

    public List<DayOfWeek> FirstDayOfWeekOptions { get; } = [
        DayOfWeek.Sunday,
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    ];
}