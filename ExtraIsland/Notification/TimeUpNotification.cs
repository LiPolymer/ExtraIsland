using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using ExCSS;
using ExtraIsland.Components;

namespace ExtraIsland.Notification;

[NotificationProviderInfo(
    "f377c816-42b1-4d1a-aa69-a67ddd6beb6d",
    "更好的倒计时",
    "ExtraIsland-更好的倒计时的提醒"
    )]
[NotificationChannelInfo(
    TimeUpChannelId,
    "倒计时结束",
    "\u1000",
    "倒计时结束后的提醒")]
public class TimeUpNotification : NotificationProviderBase {

    public const string TimeUpChannelId = "40f73a64-a0d8-480b-8026-f0a71a14d6fb";
    public TimeUpNotification(IEventService eventService) {
        EventService = eventService;
        EventService.OnTimeUp += Notify;
        EventService.OnTargetTimeChanged += Resubscribe;
        EventService.OnDetachedFromVisualTreeEventE += Resubscribe;
    }
    IEventService EventService { get; }
    void Notify(object sender, EventArgs args) {
        var a = (BetterCountdown)sender;
        ShowNotification(new NotificationRequest() {
            MaskContent = NotificationContent.CreateTwoIconsMask($"{a.Settings.Name}的时间到！")
        });
        EventService.OnTimeUp -= Notify;
    }
    public void Resubscribe(object sender, EventArgs args) {
        EventService.OnTimeUp -= Notify;
        EventService.OnTimeUp += Notify;
        
    }
    public void Unsubscribe() {
        EventService.OnTimeUp -= Notify;
        EventService.OnTargetTimeChanged -= Resubscribe;
    }
}