using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LifeLink.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (userId is not null)
        {
            var groupName = $"user_{userId}";

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                groupName
            );

            Console.WriteLine(
                $"User {userId} joined SignalR group {groupName}"
            );
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine(
            $"SignalR disconnected: {Context.ConnectionId}"
        );

        await base.OnDisconnectedAsync(exception);
    }
}