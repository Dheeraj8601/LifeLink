using LifeLink.Api.Hubs;
using LifeLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace LifeLink.Api.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(
        int userId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user ID.");

        var groupName = $"user_{userId}";

        await _hubContext.Clients
            .Group(groupName)
            .SendAsync(
                eventName,
                payload,
                cancellationToken);
    }
}