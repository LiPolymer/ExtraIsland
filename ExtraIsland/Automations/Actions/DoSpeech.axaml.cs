using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Abstractions.Services.SpeechService;
using CommunityToolkit.Mvvm.ComponentModel;
using ClassIsland.Shared;

namespace ExtraIsland.Automations.Actions;

public partial class DoSpeechSettingsControl : ActionSettingsControlBase<DoSpeechSettings> {
    public DoSpeechSettingsControl() {
        InitializeComponent();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class DoSpeechSettings : ObservableRecipient {
    public string Text { get; set; } = string.Empty;
}

[ActionInfo("extraIsland.action.doSpeech", "语音播报", "\uED53")]
public class DoSpeechAction : ActionBase<DoSpeechSettings> {
    protected override Task OnInvoke() {
        base.OnInvoke();
        string text = Settings.Text;
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("语音播报内容不能为空");
        IAppHost.GetService<ISpeechService>().EnqueueSpeechQueue(text);
        return Task.CompletedTask;
    }
}

