using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Abstractions.Services.SpeechService;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using ClassIsland.Core;
using ClassIsland.Shared;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Actions;

public class DoSpeechSettings {
    public string Text { get; set; } = string.Empty;
}

[ActionInfo("extraIsland.action.doSpeech", "语音播报", "\uE5C7")]
public class DoSpeechAction : ActionBase<DoSpeechSettings> {
    protected override Task OnInvoke() {
        base.OnInvoke();
        string? text = Settings.Text;
        if (string.IsNullOrWhiteSpace(text)) {
            throw new InvalidOperationException("语音播报内容不能为空");
        }
        
        IAppHost.GetService<ISpeechService>().EnqueueSpeechQueue(text);

        return Task.CompletedTask;
    }
}

