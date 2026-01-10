using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using ExtraIsland.Components;

namespace ExtraIsland.Shared;

[NotificationProviderInfo(
    "f377c816-42b1-4d1a-aa69-a67ddd6beb6d",
    "倒计时结束",
    "ExtraIsland中更好的倒计时到后进行的提醒"
    )]
public class TimeUpNotification : NotificationProviderBase {
    private BetterCountdown A { get; set; }
    
    public void Subscribe(BetterCountdown a) {
        
        A = a;
        A.TimeUp += Notify;

    }

    public void Unsubscribe(BetterCountdown a) {
        A = a;
        A.TimeUp -= Notify;
    }
    private void Notify(object sender,EventArgs args) {
        ShowNotification(new NotificationRequest() {
            MaskContent = NotificationContent.CreateTwoIconsMask("倒计时结束")
        });
    }

}