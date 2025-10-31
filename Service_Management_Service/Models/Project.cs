using System.ComponentModel.DataAnnotations;


namespace Service_Management_Service.Models
{
    public class Project
    {
        [Key]
        public Guid ProjectId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string ProjectTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ProjectDescription { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Requested";
        // Possible values: Requested, InProgress, Completed, Cancelled

        [Required]
        public decimal EstimatedVAT { get; set; } // VAT or Tax applied to the project

        [DataType(DataType.DateTime)]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? CompletedDate { get; set; }
    }
}
