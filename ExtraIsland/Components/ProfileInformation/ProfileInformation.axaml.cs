using Avalonia;
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
// ReSharper disable once ClassNeverInstantiated.Global
public partial class ProfileInformation : ComponentBase<ProfileInformationConfig> {
    public ProfileInformation(ILessonsService lessonsService, IProfileService profileService, IExactTimeService exactTimeService) {
        _lessonsService = lessonsService;
        _profileService = profileService;
        _exactTimeService = exactTimeService;
        
        InitializeComponent();
    }

    readonly ILessonsService _lessonsService;
    IProfileService _profileService;
    readonly IExactTimeService _exactTimeService;
    
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
        
        // 计算学期开始时间所在周的第一天（根据配置）
        DayOfWeek firstDayOfWeek = ConvertToDayOfWeek(Settings.FirstDayOfWeek);
        
        // 计算从学期开始日期到本周第一天的天数差
        int daysToStartOfWeek = ((int)orientation.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
        DateTime startDay = orientation.AddDays(-daysToStartOfWeek);
        
        // 计算当前日期所在周的最后一天
        int daysToEndOfWeek = ((int)firstDayOfWeek - (int)current.Value.DayOfWeek + 6) % 7;
        DateTime lastEnd = current.Value.AddDays(daysToEndOfWeek);
        
        TimeSpan totalWeekDelta = lastEnd - startDay;
        return Convert.ToInt32(totalWeekDelta.Days + 1) / 7;
    }
    
    DayOfWeek ConvertToDayOfWeek(FirstDayOfWeek firstDayOfWeek) {
        return firstDayOfWeek switch {
            FirstDayOfWeek.Sunday => DayOfWeek.Sunday,
            FirstDayOfWeek.Monday => DayOfWeek.Monday,
            FirstDayOfWeek.Tuesday => DayOfWeek.Tuesday,
            FirstDayOfWeek.Wednesday => DayOfWeek.Wednesday,
            FirstDayOfWeek.Thursday => DayOfWeek.Thursday,
            FirstDayOfWeek.Friday => DayOfWeek.Friday,
            FirstDayOfWeek.Saturday => DayOfWeek.Saturday,
            _ => DayOfWeek.Sunday
        };
    }
}