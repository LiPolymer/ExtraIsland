using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Avalonia.Controls;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using CommunityToolkit.Mvvm.ComponentModel;
using ClassIsland.Shared;
using ExtraIsland.ConfigHandlers;
using ExtraIsland.Notification;
using ExtraIsland.Shared;

namespace ExtraIsland.Automations.Actions;

public partial class DutyNotifySettingsControl : ActionSettingsControlBase<DutyNotifySettings> {
    public DutyNotifySettingsControl() {
        InitializeComponent();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class DutyNotifySettings : ObservableRecipient {
    public string Format { get; set; } = "当前值日:{0} · 下次轮换:{1}";
    public bool IsTopmostEnabled { get; set; }
    public bool WaitForCompletion { get; set; }
}

[ActionInfo("extraIsland.action.dutyNotify", "发送值日提示", "\uF3EF")]
public class DutyNotifyAction : ActionBase<DutyNotifySettings> {
    static EiNotificationProvider Provider { get; } =
        IAppHost.Host!.Services.GetServices<IHostedService>().OfType<EiNotificationProvider>().First();

    protected override async Task OnInvoke() {
        await base.OnInvoke();
        OnDutyPersistedConfigHandler handler = GlobalConstants.Handlers.OnDuty!;
        string current = handler.PeoplesOnDutyString;
        if (string.IsNullOrWhiteSpace(current)) return;
        string next = handler.Data.GetNextGroupOnDutyString();
        if (string.IsNullOrEmpty(next)) next = "无";
        string content;
        try {
            content = string.Format(Settings.Format,current,next);
        }
        catch (FormatException) {
            content = $"当前值日: {current}";
        }
        Task notificationTask = Provider.ShowDutyNotificationAsync(content, "\uF3EE", "", Settings.IsTopmostEnabled);
        if (!Settings.WaitForCompletion) return;
        await notificationTask;
    }
}
