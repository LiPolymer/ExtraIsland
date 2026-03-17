using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

public partial class ProfileInformationConfig : ObservableObject {
    
    [ObservableProperty]
    ProfileInformationType _type = ProfileInformationType.WeekOfSemester;
    
    [ObservableProperty]
    bool _isShortModeEnabled;
    
    [ObservableProperty]
    DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;
}

public enum ProfileInformationType {
    [Description("周数")]
    WeekOfSemester,
    [Description("单双周")]
    ParityOfWeek
}