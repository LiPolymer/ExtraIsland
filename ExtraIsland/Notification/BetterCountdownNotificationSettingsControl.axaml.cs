using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;

namespace ExtraIsland.Notification;

public partial class BetterCountdownNotificationSettingsControl :NotificationProviderControlBase<BetterCountdownNotificationSettings> {
    public BetterCountdownNotificationSettingsControl() {
        InitializeComponent();
    }
}