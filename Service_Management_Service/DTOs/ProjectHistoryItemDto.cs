// DTOs/ProjectHistoryItemDto.cs
namespace Service_Management_Service.DTOs;

public class ProjectHistoryItemDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal Price { get; set; }
    public string EndDateDisplay { get; set; } = null!;
}