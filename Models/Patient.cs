using System.ComponentModel.DataAnnotations;

namespace MaterniTrack.Models;

public class Patient
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Please enter a valid age between 1 and 120")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Contact number is required")]
    [Display(Name = "Contact Number")]
    [StringLength(20)]
    public string Contact { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [Display(Name = "Medical History")]
    public string? MedicalHistory { get; set; }

    [StringLength(200)]
    public string? Allergies { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "active"; // "active" | "inactive"

    [Display(Name = "Date Added")]
    public DateOnly DateAdded { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
