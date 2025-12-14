using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;

namespace ExtraIsland.Components;

[ComponentInfo(
                  "96808CA3-FEB8-469D-B7EC-F989C2826EE3",
                  "档案信息",
                  "\uE4B2",
                  "展示当前档案信息"
              )]
public partial class ProfileInformation : ComponentBase<ProfileInformationConfig> {
    public ProfileInformation(ILessonsService lessonsService, IProfileService profileService, IExactTimeService exactTimeService) {
        _lessonsService = lessonsService;
        _profileService = profileService;
        _exactTimeService = exactTimeService;
        
        InitializeComponent();
    }

    readonly ILessonsService _lessonsService;
    IProfileService _profileService;
    IExactTimeService _exactTimeService;
    
    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        _lessonsService.PostMainTimerTicked += LessonsServiceOnPostMainTimerTicked;
    }

    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        _lessonsService.PostMainTimerTicked -= LessonsServiceOnPostMainTimerTicked;
    }
    
    void LessonsServiceOnPostMainTimerTicked(object? sender,EventArgs e) {
        string result;
        switch (Settings.Type) {
            case ProfileInformationType.WeekOfSemester:
                result = Settings.IsShortModeEnabled ? GetWeekInSemester().ToString() 
                    : $"第{GetWeekInSemester()}周";
                break;
            case ProfileInformationType.ParityOfWeek:
                string parity = GetWeekInSemester() % 2 == 0 ? "双" : "单";
                result = Settings.IsShortModeEnabled ? parity : parity + "周";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Dispatcher.UIThread.Invoke(() => TextLabel.Content = result);
    }

    int GetWeekInSemester(DateTime? current = null) {
        current ??= _exactTimeService.GetCurrentLocalDateTime();
        DateTime orientation = (DateTime)((dynamic)AppBase.Current).Settings.SingleWeekStartTime;
        int oriWeek = orientation.DayOfWeek == DayOfWeek.Sunday ? 6 : Convert.ToInt32(orientation.DayOfWeek) - 1;
        
        //regulate
        string firstDay = orientation.AddDays(-oriWeek).ToString("yyyy-MM-dd");
        DateTime startMonday = Convert.ToDateTime(firstDay);
        int lastDelta = current.Value.DayOfWeek != DayOfWeek.Sunday ? 7 - Convert.ToInt32(current.Value.DayOfWeek) : 0;
        DateTime lastEnd = current.Value.AddDays(lastDelta);
        
        TimeSpan totalWeekDelta = lastEnd - startMonday;
        return Convert.ToInt32(totalWeekDelta.Days + 1) / 7;
    }
}