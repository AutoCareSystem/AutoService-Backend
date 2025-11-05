using Microsoft.AspNetCore.SignalR;

namespace Notifications_Service.Hubs;

public class NotificationHub : Hub
{
    // Called when a client connects
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"✅ Client connected: {Context.ConnectionId} | UserId: {userId}");
        await base.OnConnectedAsync();
    }

    // Called when a client disconnects
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"❌ Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Method clients can call to register with their userId
    public async Task RegisterUser(string userId)
    {
        // Add connection to a group named after the userId
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        Console.WriteLine($"🔔 User {userId} registered for notifications");
    }

    // Send notification to specific user
    public async Task SendNotificationToUser(string userId, string message)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", message);
    }

    // Broadcast to all connected clients (admin feature)
    public async Task BroadcastNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}
