namespace LifeLink.Application.Interfaces.Services;

public interface INotificationService
{
    Task NotifyUserAsync(
        int userId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default);
}