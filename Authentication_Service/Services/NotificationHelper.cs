using System.Text;
using System.Text.Json;

namespace backend_EAD.Services
{
    public class NotificationHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationHelper> _logger;
        private readonly string _notificationServiceUrl;

        public NotificationHelper(IHttpClientFactory httpClientFactory, ILogger<NotificationHelper> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _notificationServiceUrl = configuration["NotificationService:Url"] ?? "http://localhost:5295";
        }

        public async Task SendProfileUpdateNotificationAsync(string userId, string userType, List<string> updatedFields)
        {
            try
            {
                var notification = new
                {
                    UserID = userId,
                    NotificationType = "ProfileUpdate",
                    Title = $"{userType} Profile Updated",
                    Message = $"Your profile has been successfully updated. Changed fields: {string.Join(", ", updatedFields)}",
                    EntityType = userType,
                    EntityID = 0,
                    Metadata = JsonSerializer.Serialize(new { UpdatedFields = updatedFields, UpdatedAt = DateTime.UtcNow })
                };

                var httpClient = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonSerializer.Serialize(notification), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_notificationServiceUrl}/api/Notifications", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Notification sent successfully for {userType} {userId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to send notification. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
                // Don't throw - notification failure shouldn't break profile update
            }
        }
    }
}
