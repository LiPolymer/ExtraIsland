using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Triggers;

[TriggerInfo("extraIsland.trigger.timePassed", "间隔触发", "\uE165")]
// ReSharper disable once ClassNeverInstantiated.Global
public class TimePassed : TriggerBase<TimePassedConfig> {
    readonly ILessonsService _lessonsService;

    public TimePassed(ILessonsService lessonsService) {
        _lessonsService = lessonsService;
    }

    void Check(object? sender,EventArgs e) {
        TimeSpan delta = EiUtils.GetDateTimeSpan(Settings.LastTriggered, DateTime.Now);
        if (delta <= Settings.TimeGap) return;
        Settings.LastTriggered = DateTime.Now;
        Trigger();
    }
    
    public override void Loaded() {
        _lessonsService.PostMainTimerTicked += Check;
    }

    public override void UnLoaded() {
        _lessonsService.PostMainTimerTicked -= Check;
    }
}