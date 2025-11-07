public class UpdateProjectDto
{
    public int AppointmentID { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan Time { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string ProjectTitle { get; set; } = null!;
    public string? ProjectDescription { get; set; }
    public string? EmployeeID { get; set; }  
}