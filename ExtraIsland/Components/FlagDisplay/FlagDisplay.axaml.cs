using System.ComponentModel.DataAnnotations.Schema;
using Avalonia;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ExtraIsland.Automations.Rules;
using ExtraIsland.Shared;

namespace ExtraIsland.Components;

[ComponentInfo(
                  "0ce2fe37-5f79-41c1-a2fe-375f7901c182",
                  "标志展示",
                  "\uE843",
                  "展示由设标志行动设定的特定标志内容"
              )]
// ReSharper disable once ClassNeverInstantiated.Global
public partial class FlagDisplay : ComponentBase<FlagDisplayConfig> {
    public FlagDisplay(ILessonsService lessonsService) {
        _lessonsService = lessonsService;
        InitializeComponent();
        _labelAnimator = new Animators.GenericContentSwapAnimator(TextLabel);
    }

    readonly ILessonsService _lessonsService;
    readonly Animators.GenericContentSwapAnimator _labelAnimator;

    void OnAttachedToVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        _lessonsService.PostMainTimerTicked += LessonsServiceOnPostMainTimerTicked;
        Update();
    }

    void OnDetachedFromVisualTree(object? sender,VisualTreeAttachmentEventArgs e) {
        _lessonsService.PostMainTimerTicked -= LessonsServiceOnPostMainTimerTicked;
    }

    void LessonsServiceOnPostMainTimerTicked(object? sender,EventArgs e) {
        Update();
    }

    void Update() {
        string s = GetFlagContent();
        _labelAnimator.Update(s,Settings.IsAnimationEnabled,Settings.IsSwapAnimationEnabled);
    }

    string GetFlagContent() {
        if (Flag.Flags.TryGetValue(Settings.TargetFlag, out string? flag)) return flag!;
        return GlobalConstants.Handlers.PersistedFlagHandler!.FlagsTable
            .TryGetValue(Settings.TargetFlag,out string? pFlag) ? pFlag : Settings.FallbackText;
    }
}
