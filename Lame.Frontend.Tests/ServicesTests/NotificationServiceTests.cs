using Lame.Frontend.Services;

namespace Lame.Frontend.Tests.ServicesTests;

public class NotificationServiceTests
{
    [Fact]
    public void EmitNotification_AddsNotificationAndRaisesEvent()
    {
        // Arrange
        var service = new NotificationService();
        var notification = new Notification();
        var eventRaised = false;
        service.NotificationsChanged += () => eventRaised = true;

        // Act
        service.EmitNotification(notification);

        // Assert
        Assert.Contains(notification, service.Notifications);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Notifications_CollectionIsEmptyOnInitialization()
    {
        // Arrange
        var service = new NotificationService();

        // Act
        var notifications = service.Notifications;

        // Assert
        Assert.Empty(notifications);
    }
}