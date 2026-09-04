using System.ComponentModel.DataAnnotations;

namespace MaterniTrack.Models;

public class ClinicProfileSetting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string ClinicName { get; set; } = "Real-Mendoza Maternity Clinic";

    [MaxLength(150)]
    public string Specialization { get; set; } = "Maternal & Neonatal Healthcare Facility";

    [Required]
    [MaxLength(100)]
    public string MedicalDirector { get; set; } = "Dr. Leyo Mendoza, MD, OB-GYN";

    [MaxLength(50)]
    public string DohLicense { get; set; } = "DOH-NCR-CL-2026-08492";

    [MaxLength(50)]
    public string PhilHealthAccreditation { get; set; } = "PH-ACCRED-9482710";

    [Required]
    [MaxLength(50)]
    public string ContactPhone { get; set; } = "+63 917 123 4567";

    [MaxLength(100)]
    public string ContactEmail { get; set; } = "admin@clinic.local";

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = "District 2, Marikina City, Metro Manila";

    [MaxLength(100)]
    public string OperatingHours { get; set; } = "Monday - Saturday: 8:00 AM - 5:00 PM (Emergency: 24/7)";

    [MaxLength(50)]
    public string EmergencyHotline { get; set; } = "161 / (02) 8646 0427";

    // Notification Toggles
    public bool SmsRemindersEnabled { get; set; } = true;
    public bool EmailRemindersEnabled { get; set; } = true;
    public bool ConflictAlertsEnabled { get; set; } = true;
    public bool HighRiskFlagsEnabled { get; set; } = true;
    public bool LowStockAlertsEnabled { get; set; } = true;
    public bool DailyDigestEnabled { get; set; } = false;
}
