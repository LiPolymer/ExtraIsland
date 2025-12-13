using System.Collections.ObjectModel;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared.Enums;
using ClassIsland.Shared.Models.Profile;
using ExtraIsland.Shared;
using Microsoft.Extensions.Logging;

namespace ExtraIsland.Automations.Rules;

public static class IsDoubleLesson {
    public static bool Rule(object? _) {
        if (GlobalConstants.HostInterfaces.LessonsService == null) return false;
        ILessonsService ls = GlobalConstants.HostInterfaces.LessonsService;
        if (GlobalConstants.HostInterfaces.ProfileService == null) return false;
        IProfileService ps = GlobalConstants.HostInterfaces.ProfileService;
        switch (ls.CurrentState) {
            case TimeState.OnClass:
                return GetSubjectGuid(ls.CurrentSubject!) == GetSubjectGuid(ls.NextClassSubject);
            case TimeState.Breaking or TimeState.PrepareOnClass or TimeState.None:
                if (ls.CurrentClassPlan == null) return false;
                ClassPlan ccp = ls.CurrentClassPlan;
                if (ccp.TimeLayout == null) return false;
                ObservableCollection<TimeLayoutItem> tls = ccp.TimeLayout.Layouts;
                if (ls.CurrentSelectedIndex == -1) return false;
                int index = tls.Where(ti => ti.TimeType == 0)
                    .ToList()
                    .IndexOf(tls
                                 .Take(ls.CurrentSelectedIndex)
                                 .Last(ti => ti.TimeType == 0));
                GlobalConstants.HostInterfaces.PluginLogger?.LogDebug($"INDEX {index}");
                return ccp.Classes[index].SubjectId == GetSubjectGuid(ls.NextClassSubject);
            default:
                return false;
        }
    }

    static Guid? GetSubjectGuid(Subject subject) {
        return GlobalConstants.HostInterfaces.ProfileService?.Profile.Subjects
            .FirstOrDefault(kvp => kvp.Value == subject, default).Key;
    }
}