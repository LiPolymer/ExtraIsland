using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;

namespace ExtraIsland.Notification;

[NotificationProviderInfo(
    "f377c816-42b1-4d1a-aa69-a67ddd6beb6d",
    "ExtraIsland 提醒",
    "\uEA37",
    "ExtraIsland 提供的提醒服务"
    )]
[NotificationChannelInfo(
    TimeUpChannelId,
    "倒计时结束",
    "\uF361",
    "倒计时结束后的提醒")]
[NotificationChannelInfo(
    DutyReminderChannelId,
    "值日生提醒",
    "\uF3EF",
    "值日轮换提示")]
public class EiNotificationProvider : NotificationProviderBase {

    const string TimeUpChannelId = "40f73a64-a0d8-480b-8026-f0a71a14d6fb";
    public const string DutyReminderChannelId = "d0b83640-bec1-42e0-a3ab-cef698413f9e";

    delegate void TwoIconsMaskNotify(string content, string leftIcon, string rightIcon, bool isTopmost);

    static event TwoIconsMaskNotify? OnNotify;

    public static void Notify(string content, string leftIcon = "", string rightIcon = "", bool isTopmost = false) {
        OnNotify?.Invoke(content, leftIcon, rightIcon, isTopmost);
    }

    public EiNotificationProvider() {
        OnNotify += DoNotify;
    }

    void DoNotify(string content, string leftIcon, string rightIcon, bool isTopmost) {
        Channel(TimeUpChannelId).ShowNotification(BuildRequest(content, leftIcon, rightIcon, isTopmost));
    }

    public async Task ShowDutyNotificationAsync(string content, string leftIcon = "", string rightIcon = "", bool isTopmost = false) {
        NotificationRequest request = BuildRequest(content, leftIcon, rightIcon, isTopmost);
        request.ChannelId = Guid.Parse(DutyReminderChannelId);
        await ShowNotificationAsync(request);
    }

    static NotificationRequest BuildRequest(string content, string leftIcon, string rightIcon, bool isTopmost) {
        return new NotificationRequest {
            MaskContent = NotificationContent.CreateTwoIconsMask(content, leftIcon, rightIcon),
            RequestNotificationSettings = {
                IsSettingsEnabled = isTopmost,
                IsNotificationTopmostEnabled = isTopmost
            }
        };
    }
}
