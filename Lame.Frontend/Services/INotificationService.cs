using System.Collections.ObjectModel;

namespace Lame.Frontend.Services;

public enum NotificationType
{
    Info,
    Warning,
    Success,
    Failure
}

public class Notification
{
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public interface INotificationService
{
    public ObservableCollection<Notification> Notifications { get; }
    event Action NotificationsChanged;

    public void EmitNotification(Notification notification);
}