using System.ComponentModel.DataAnnotations;

namespace MaterniTrack.Models;

public class Appointment
{
    public int Id { get; set; }

    public int? PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required(ErrorMessage = "Patient name is required")]
    [Display(Name = "Patient Full Name")]
    [StringLength(100)]
    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [Display(Name = "Contact Number")]
    [StringLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Appointment date is required")]
    [Display(Name = "Appointment Date")]
    [DataType(DataType.Date)]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Appointment time is required")]
    [Display(Name = "Appointment Time")]
    [StringLength(10)]
    public string AppointmentTime { get; set; } = "09:00"; // e.g. "09:00", "14:30"

    [Required(ErrorMessage = "Appointment type is required")]
    [Display(Name = "Appointment Type")]
    [StringLength(50)]
    public string AppointmentType { get; set; } = "prenatal"; // "prenatal", "postnatal", "consultation", "followup", "others"

    [Required(ErrorMessage = "Please assign a staff member")]
    [Display(Name = "Assigned Staff / Doctor")]
    [StringLength(100)]
    public string AssignedStaff { get; set; } = "dr-real-mendoza"; // e.g. "dr-real-mendoza", "juan-santos", "maria-reyes", "rose-garcia"

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "pending"; // "pending", "confirmed", "done", "cancelled"

    public DateOnly DateCreated { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
