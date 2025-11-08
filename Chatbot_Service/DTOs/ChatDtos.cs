namespace Chatbot_Service.DTOs;

public class ChatRequestDto
{
    public string Message { get; set; } = null!;
    public DateTime? PreferredDate { get; set; }
}

public class ChatResponseDto
{
    public string Message { get; set; } = null!;
    public List<TimeSlotDto>? AvailableSlots { get; set; }
}

public class TimeSlotDto
{
    public DateTime Date { get; set; }
    public string Time { get; set; } = null!;
    public bool IsAvailable { get; set; }
}

public class AvailableTimeSlotsDto
{
    public DateTime Date { get; set; }
    public List<string> AvailableSlots { get; set; } = new();
}
