using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeLogging_Service.Models
{
    [Table("time_logs")]
    public class TimeLog
    {
        [Key]
        [Column("time_log_id")]
        public int TimeLogID { get; set; }

        [Required]
        [Column("appointment_id")]
        public int AppointmentID { get; set; }

        [Column("employee_id")]
        public int? EmployeeID { get; set; }

        [Column("started_at")]
        public DateTimeOffset? StartedAt { get; set; }

        [Column("ended_at")]
        public DateTimeOffset? EndedAt { get; set; }

        [Column("duration_minutes")]
        public double? DurationMinutes { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
