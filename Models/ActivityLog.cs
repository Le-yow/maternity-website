using System.ComponentModel.DataAnnotations;

namespace MaterniTrack.Models;

public class ActivityLog
{
    [Key]
    public int Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = "General"; // Authentication, Patients, Appointments, Inventory, Settings

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PerformedBy { get; set; } = "System";

    [MaxLength(500)]
    public string Details { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Severity { get; set; } = "Info"; // Info, Success, Warning, Danger
}
