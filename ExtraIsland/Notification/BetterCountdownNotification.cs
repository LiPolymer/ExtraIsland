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
    "\uE84C",
    "倒计时结束后的提醒")]
[NotificationChannelInfo(
    TimeNodeChannelId,
    "时间节点",
    "\uf35a",
    "到达时间节点时的提醒")]
public class BetterCountdownNotification : NotificationProviderBase<BetterCountdownNotificationSettings> {

    const string TimeUpChannelId = "40f73a64-a0d8-480b-8026-f0a71a14d6fb";
    const string TimeNodeChannelId = "a6b1ebc0-5d17-4152-b8a7-18f3dd15668b";

    delegate void TwoIconsMaskNotify(string name, string message, int mode, string leftIcon, string rightIcon, string timeDistance);
    
    static event TwoIconsMaskNotify? OnNotify;

    public static void Notify(string name, string message, int mode, string leftIcon = "", string rightIcon = "", string timeDistance="") {
        OnNotify?.Invoke(name, message, mode, leftIcon, rightIcon, timeDistance);
    }
    
    public BetterCountdownNotification() {
        OnNotify += DoNotify;
    }
    
    void DoNotify(string name, string content, int mode, string leftIcon, string rightIcon, string timeDistance) {
        if (mode == 0) {
            Channel(TimeUpChannelId).ShowNotification(new NotificationRequest() {
                MaskContent = NotificationContent.CreateTwoIconsMask(content == "" ? Settings.Message.Replace("{n}", name) :
                    content.Replace("{n}", name),leftIcon,rightIcon)
            });
        } else if (mode == 1) {
            Channel(TimeNodeChannelId).ShowNotification(new NotificationRequest() {
                MaskContent = NotificationContent.CreateTwoIconsMask(content.Replace("{n}", name).Replace("{t}", timeDistance),leftIcon,rightIcon)
            });
        }
    }
}