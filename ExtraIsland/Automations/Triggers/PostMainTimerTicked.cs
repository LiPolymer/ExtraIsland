using System.DirectoryServices.ActiveDirectory;
using Windows.Win32;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ExtraIsland.Shared;
using MaterialDesignThemes.Wpf;

namespace ExtraIsland.Automations.Triggers;

[TriggerInfo("extraIsland.trigger.postMainTimerTicked", "主计时器触发时", PackIconKind.TimerPlay)]
// ReSharper disable once ClassNeverInstantiated.Global
public class PostMainTimerTicked : TriggerBase {
    void Fire(object? sender,EventArgs e) {
        Trigger();
    }
    
    public override void Loaded() {
        GlobalConstants.HostInterfaces.LessonsService!.PostMainTimerTicked += Fire;
    }

    public override void UnLoaded() {
        GlobalConstants.HostInterfaces.LessonsService!.PostMainTimerTicked -= Fire;
    }
}