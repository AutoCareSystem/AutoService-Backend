// DTOs/CustomerVehicleSummaryDto.cs
namespace Service_Management_Service.DTOs;

public class CustomerVehicleSummaryDto
{
    public decimal TotalSpent { get; set; } = 0m;
    public int TotalVehicles { get; set; } = 0;
    public int CompletedCount { get; set; } = 0;
    public int PendingCount { get; set; } = 0;
}