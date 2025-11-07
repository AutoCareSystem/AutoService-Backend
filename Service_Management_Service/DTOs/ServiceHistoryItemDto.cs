// DTOs/ServiceHistoryItemDto.cs
namespace Service_Management_Service.DTOs;

public class ServiceHistoryItemDto
{
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;  // Pending, Approved, Completed
    public decimal Price { get; set; }
    public string EndDateDisplay { get; set; } = null!;  // "2025-11-10 11:00" or "Not completed yet"
}