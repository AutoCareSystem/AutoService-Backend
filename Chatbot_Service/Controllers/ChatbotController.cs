using Microsoft.AspNetCore.Mvc;
using Chatbot_Service.DTOs;
using Chatbot_Service.Services;

namespace Chatbot_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly TimeSlotService _timeSlotService;
    private readonly GeminiService _geminiService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(
        TimeSlotService timeSlotService,
        GeminiService geminiService,
        ILogger<ChatbotController> logger)
    {
        _timeSlotService = timeSlotService;
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Chat with the bot to get available time slots
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Message cannot be empty" });
            }

            // Check if user is asking about available time slots
            var isAskingAboutSlots = IsAskingAboutTimeSlots(request.Message);

            string context = "";
            List<AvailableTimeSlotsDto>? availableSlots = null;

            if (isAskingAboutSlots)
            {
                // Get available time slots from database
                availableSlots = await _timeSlotService.GetAvailableTimeSlots(request.PreferredDate, 7);
                context = await _timeSlotService.GetAvailableSlotsContext(request.PreferredDate);
            }
            else
            {
                context = "The user is asking a general question about the auto service center. " +
                         "Please provide helpful information about services, appointment booking, or direct them to ask about available time slots.";
            }

            // Get AI response from Gemini
            var aiResponse = await _geminiService.GenerateResponse(request.Message, context);

            var response = new ChatResponseDto
            {
                Message = aiResponse,
                AvailableSlots = availableSlots?.SelectMany(day =>
                    day.AvailableSlots.Select(time => new TimeSlotDto
                    {
                        Date = day.Date,
                        Time = time,
                        IsAvailable = true
                    })).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing chat request: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get available time slots for a specific date range
    /// </summary>
    [HttpGet("available-slots")]
    public async Task<ActionResult<List<AvailableTimeSlotsDto>>> GetAvailableSlots(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] int days = 7)
    {
        try
        {
            var slots = await _timeSlotService.GetAvailableTimeSlots(startDate, days);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting available slots: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while fetching available slots" });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Chatbot Service",
            timestamp = DateTime.UtcNow
        });
    }

    private bool IsAskingAboutTimeSlots(string message)
    {
        var lowerMessage = message.ToLower();
        var keywords = new[] {
            "available", "time", "slot", "appointment", "book", "schedule",
            "free", "when", "date", "time slots", "availability"
        };

        return keywords.Any(keyword => lowerMessage.Contains(keyword));
    }
}
