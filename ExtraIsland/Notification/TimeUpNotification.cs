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

    const string TimeUpChannelId = "40f73a64-a0d8-480b-8026-f0a71a14d6fb";
    public TimeUpNotification(IEventService eventService) {
        EventService = eventService;
        EventService.OnTimeUp += Notify;
        EventService.OnTargetTimeChanged += Resubscribe;
        EventService.OnDetachedFromVisualTreeEventE += Unsubscribe;
        EventService.OnAttachedToVisualTreeE += Resubscribe;
    }
    IEventService EventService { get; }
    void Notify(object? sender, EventArgs args) {
        if ((BetterCountdown?)sender == null) return;
        BetterCountdown betterCountdown = (BetterCountdown)sender;
        Channel(TimeUpChannelId).ShowNotification(new NotificationRequest() {
            MaskContent = NotificationContent.CreateTwoIconsMask($"{betterCountdown.Settings.Name}{betterCountdown.Settings.Message}")
        });
        EventService.OnTimeUp -= Notify;
    }
    void Resubscribe(object? sender, EventArgs args) {
        EventService.OnTimeUp -= Notify;
        EventService.OnTimeUp += Notify;
        EventService.OnTargetTimeChanged -= Resubscribe;
        EventService.OnTargetTimeChanged += Resubscribe;
    }
    void Unsubscribe(object? sender, EventArgs args) {
        EventService.OnTimeUp -= Notify;
        EventService.OnTargetTimeChanged -= Resubscribe;
    }
}