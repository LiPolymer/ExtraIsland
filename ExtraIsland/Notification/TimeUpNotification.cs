using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;

namespace ExtraIsland.Notification;

[NotificationProviderInfo(
    "f377c816-42b1-4d1a-aa69-a67ddd6beb6d",
    "更好的倒计时提醒",
    "更好的倒计时到点提醒"
    )]
[NotificationChannelInfo(
    TimeUpChannelId,
    "倒计时结束",
    "\u1000",
    "倒计时结束后的提醒")]
public class TimeUpNotification : NotificationProviderBase {

    const string TimeUpChannelId = "40f73a64-a0d8-480b-8026-f0a71a14d6fb";

    delegate void TwoIconsMaskNotify(string content, string leftIcon, string rightIcon);
    
    static event TwoIconsMaskNotify? OnNotify;

    public static void Notify(string content, string leftIcon = "", string rightIcon = "") {
        OnNotify?.Invoke(content, leftIcon, rightIcon);
    }
    
    public TimeUpNotification() {
        OnNotify += DoNotify;
    }
    
    void DoNotify(string content, string leftIcon, string rightIcon) {
        Channel(TimeUpChannelId).ShowNotification(new NotificationRequest() {
            MaskContent = NotificationContent.CreateTwoIconsMask(content, leftIcon, rightIcon)
        });
    }
}