# Notification Service with SignalR

## Overview

Real-time notification service built with ASP.NET Core and SignalR. Monitors appointment changes and sends real-time notifications to users.

## Features

- ✅ **Real-time Notifications** via SignalR
- ✅ **Appointment Monitoring** - Automatically detects status changes
- ✅ **Database Persistence** - All notifications stored in PostgreSQL
- ✅ **User-specific Groups** - Notifications sent only to relevant users
- ✅ **Background Service** - Continuous monitoring of appointment changes
- ✅ **RESTful API** - Full CRUD operations for notifications

## Architecture

### Components

1. **NotificationHub** - SignalR hub for real-time communication
2. **NotificationService** - Business logic for notifications
3. **AppointmentMonitorService** - Background service monitoring appointments
4. **NotificationsController** - REST API endpoints

### Notification Flow

```
Appointment Created/Updated
    ↓
AppointmentMonitorService detects change
    ↓
NotificationService creates notification in DB
    ↓
SignalR sends real-time notification to user
    ↓
User receives notification instantly
```

## API Endpoints

### Get User Notifications

```http
GET /api/notifications/user/{userId}?unreadOnly=false&limit=50
```

### Get Notification Summary

```http
GET /api/notifications/user/{userId}/summary
```

### Create Manual Notification

```http
POST /api/notifications
Content-Type: application/json

{
  "userId": "user-guid",
  "notificationType": "Appointment",
  "title": "Test Notification",
  "message": "This is a test message",
  "entityType": "Appointment",
  "entityID": 123
}
```

### Mark Notifications as Read

```http
PUT /api/notifications/user/{userId}/mark-read
Content-Type: application/json

{
  "notificationIDs": [1, 2, 3]
}
```

### Mark All as Read

```http
PUT /api/notifications/user/{userId}/mark-all-read
```

### Delete Notification

```http
DELETE /api/notifications/user/{userId}/{notificationId}
```

## SignalR Connection

### JavaScript/TypeScript Client Example

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5295/notificationHub?userId=YOUR_USER_ID")
  .withAutomaticReconnect()
  .build();

// Receive notifications
connection.on("ReceiveNotification", (notification) => {
  console.log("New notification:", notification);
  // Update UI, show toast, etc.
});

// Notifications marked as read
connection.on("NotificationsMarkedAsRead", (notificationIds) => {
  console.log("Marked as read:", notificationIds);
});

// All notifications marked as read
connection.on("AllNotificationsMarkedAsRead", () => {
  console.log("All notifications marked as read");
});

await connection.start();
```

### C# Client Example

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5295/notificationHub?userId=YOUR_USER_ID")
    .WithAutomaticReconnect()
    .Build();

connection.On<NotificationDto>("ReceiveNotification", notification =>
{
    Console.WriteLine($"Received: {notification.Title}");
});

await connection.StartAsync();
```

## Notification Types

### Appointment Notifications

- **Created** - When a new appointment is created
- **Approved** - When employee accepts appointment
- **Completed** - When appointment is marked complete
- **Cancelled** - When appointment is cancelled
- **Rejected** - When appointment is declined

## Database Schema

### Notifications Table

```sql
CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY,
    UserID VARCHAR(450) NOT NULL,
    NotificationType VARCHAR(50) NOT NULL,
    Title VARCHAR(100) NOT NULL,
    Message VARCHAR(500) NOT NULL,
    EntityType VARCHAR(50),
    EntityID INT,
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP NOT NULL,
    ReadAt TIMESTAMP,
    Metadata VARCHAR(1000)
);
```

## Setup Instructions

1. **Restore packages**

   ```bash
   dotnet restore
   ```

2. **Create migration** (if needed)

   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. **Update database**

   ```bash
   dotnet ef database update
   ```

4. **Run the service**

   ```bash
   dotnet run
   ```

5. **Access Swagger**
   ```
   http://localhost:5295/swagger
   ```

## Environment Variables

Create a `.env` file with:

```
DATABASE_URL=Host=your-host;Database=AutoService;Username=user;Password=pass;SSL Mode=Require;Trust Server Certificate=true;
```

## Monitoring

The `AppointmentMonitorService` runs in the background and checks for appointment changes every 10 seconds. It:

- Detects new appointments
- Tracks status changes
- Sends notifications to customers
- Sends notifications to assigned employees

## Testing

### Test Manual Notification

```bash
curl -X POST http://localhost:5295/api/notifications \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "test-user-id",
    "notificationType": "Test",
    "title": "Test Notification",
    "message": "This is a test"
  }'
```

### Test SignalR Connection

Use the Swagger UI or a SignalR client to connect and receive real-time notifications.

## Port Configuration

- HTTP: 5295
- HTTPS: 7217

## Dependencies

- ASP.NET Core 9.0
- Entity Framework Core 9.0
- Npgsql (PostgreSQL)
- SignalR
- DotNetEnv

## Future Enhancements

- [ ] Email notifications
- [ ] SMS notifications
- [ ] Push notifications for mobile
- [ ] Notification preferences per user
- [ ] Notification templates
- [ ] Scheduled notifications
