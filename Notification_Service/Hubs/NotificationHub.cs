using Microsoft.AspNetCore.SignalR;

namespace Notification_Service.Hubs;

public class NotificationHub : Hub
{
    private static readonly Dictionary<string, string> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"✅ User {userId} connected with connection ID: {Context.ConnectionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections.Remove(userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"❌ User {userId} disconnected");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        _userConnections[userId] = Context.ConnectionId;
        Console.WriteLine($"👤 User {userId} joined their group");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        _userConnections.Remove(userId);
        Console.WriteLine($"👤 User {userId} left their group");
    }

    public static string? GetConnectionId(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
    }

    public static bool IsUserConnected(string userId)
    {
        return _userConnections.ContainsKey(userId);
    }
}
