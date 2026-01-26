using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using ExtraIsland.Components;

namespace ExtraIsland.Notification;

[NotificationProviderInfo(
    "f377c816-42b1-4d1a-aa69-a67ddd6beb6d",
    "倒计时结束",
    "ExtraIsland中更好的倒计时到后进行的提醒"
    )]
public class TimeUpNotification : NotificationProviderBase {
    BetterCountdown BetterCountdown { get; }
    
    public TimeUpNotification(BetterCountdown betterCountdown) {
        BetterCountdown = betterCountdown;
        BetterCountdown.TimeUp += Notify;
        BetterCountdown.Settings.OnTargetDateTimeChanged += Resubscribe;
    }
    
    void Notify(object sender, EventArgs args) {
        ShowNotification(new NotificationRequest() {
            MaskContent = NotificationContent.CreateTwoIconsMask("倒计时结束")
        });
        BetterCountdown.TimeUp -= Notify;
    }
    public void Resubscribe(object sender, EventArgs args) {
        BetterCountdown.TimeUp -= Notify;
        BetterCountdown.TimeUp += Notify;
        
    }
    public void Unsubscribe() {
        BetterCountdown.TimeUp -= Notify;
        BetterCountdown.Settings.OnTargetDateTimeChanged -= Resubscribe;
    }
}