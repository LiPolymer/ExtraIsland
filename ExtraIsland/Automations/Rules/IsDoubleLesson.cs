using System.Collections.ObjectModel;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared.Enums;
using ClassIsland.Shared.Models.Profile;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Automations.Rules;

public static class IsDoubleLesson {
    public static bool Rule(object? _,ILessonsService lessonsService,IProfileService profileService,ILogger logger) {
        switch (lessonsService.CurrentState) {
            case TimeState.OnClass:
                return GetSubjectGuid(lessonsService.CurrentSubject!,profileService) == GetSubjectGuid(lessonsService.NextClassSubject,profileService);
            case TimeState.Breaking or TimeState.PrepareOnClass or TimeState.None:
                if (lessonsService.CurrentClassPlan == null) return false;
                ClassPlan ccp = lessonsService.CurrentClassPlan;
                if (ccp.TimeLayout == null) return false;
                ObservableCollection<TimeLayoutItem> tls = ccp.TimeLayout.Layouts;
                if (lessonsService.CurrentSelectedIndex == -1) return false;
                int index = tls.Where(ti => ti.TimeType == 0)
                    .ToList()
                    .IndexOf(tls
                                 .Take(lessonsService.CurrentSelectedIndex)
                                 .Last(ti => ti.TimeType == 0));
                logger.LogDebug($"INDEX {index}");
                return ccp.Classes[index].SubjectId == GetSubjectGuid(lessonsService.NextClassSubject,profileService);
            default:
                return false;
        }
    }

    static Guid? GetSubjectGuid(Subject subject,IProfileService profileService) {
        return profileService.Profile.Subjects
            .FirstOrDefault(kvp => kvp.Value == subject, default).Key;
    }
}
