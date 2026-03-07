using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Components;

public partial class ProfileInformationConfig : ObservableObject {
    
    [ObservableProperty]
    ProfileInformationType _type = ProfileInformationType.WeekOfSemester;
    
    [ObservableProperty]
    bool _isShortModeEnabled;
    
    [ObservableProperty]
    FirstDayOfWeek _firstDayOfWeek = FirstDayOfWeek.Sunday;
}

public enum ProfileInformationType {
    [Description("周数")]
    WeekOfSemester,
    [Description("单双周")]
    ParityOfWeek
}

public enum FirstDayOfWeek {
    [Description("周日")]
    Sunday,
    [Description("周一")]
    Monday,
    [Description("周二")]
    Tuesday,
    [Description("周三")]
    Wednesday,
    [Description("周四")]
    Thursday,
    [Description("周五")]
    Friday,
    [Description("周六")]
    Saturday
}