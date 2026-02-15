using CommunityToolkit.Mvvm.ComponentModel;

namespace ExtraIsland.Notification;

public class BetterCountdownNotificationSettings : ObservableRecipient{
    string _message = "的时间到了";
    public string Message {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}