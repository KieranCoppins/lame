using System.Collections.ObjectModel;

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

        // Optionally remove after a delay
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