using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lame.Frontend.Services;

public class NotificationService : INotificationService
{
    public NotificationService()
    {
        Notifications = [];
    }

    public event Action? NotificationsChanged;

    public ObservableCollection<Notification> Notifications { get; }

    public void EmitNotification(Notification notification)
    {
        Notifications.Add(notification);
        NotificationsChanged?.Invoke();

        // TODO: Abstract dispatch provider for better testability
        Task.Delay(5000).ContinueWith(_ =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Notifications.Remove(notification);
                NotificationsChanged?.Invoke();
            });
        });
    }
}