using System.Collections.ObjectModel;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared.Enums;
using ClassIsland.Shared.Models.Profile;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Rules;

public static class IsDoubleLesson {
    public static bool Rule(object? _) {
        if (GlobalConstants.HostInterfaces.LessonsService == null) return false;
        ILessonsService ls = GlobalConstants.HostInterfaces.LessonsService;
        switch (ls.CurrentState) {
            case TimeState.OnClass:
                return ls.CurrentSubject?.Name != ls.NextClassSubject.Name;
            case TimeState.Breaking or TimeState.PrepareOnClass:
                if (ls.CurrentClassPlan == null) return false;
                ClassPlan ccp = ls.CurrentClassPlan;
                if (ccp.TimeLayout == null) return false;
                ObservableCollection<TimeLayoutItem> tls = ccp.TimeLayout.Layouts;
                if (ls.CurrentSelectedIndex == -1) return false;
                //todo: this part may not work
                /*
                return ccp.Classes.FirstOrDefault(i => i.SubjectId == tls.Take(ls.CurrentSelectedIndex)
                                                      .Where(ti => ti.TimeType == 0)
                                                      .Last().DefaultClassId).;*/
                return false;
            default:
                return false;
        }
    }
}