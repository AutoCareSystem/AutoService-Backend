namespace Service_Management_Service.DTOs
{
    public class CreateAppointmentDto
    {
        public int CustomerID { get; set; }
        public int VehicleID { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan Time { get; set; }
        public DateTime? EndDate { get; set; } 
        public string AppointmentType { get; set; } = null!; // "Service" or "Project"
        // Service-specific
        public string? ServiceOption { get; set; } // "Full", "Half", "Custom"
        public List<int>? CustomServiceIDs { get; set; } // For Custom Service appointments
        public int? ServicePackageID { get; set; }          // Required for Full/Half
        // Project-specific
        public string? ProjectTitle { get; set; }
        public string? ProjectDescription { get; set; }
    }
}