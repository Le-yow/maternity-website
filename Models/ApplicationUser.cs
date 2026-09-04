using Microsoft.AspNetCore.Identity;

namespace MaterniTrack.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string ClinicRole { get; set; } = "Staff"; // "Doctor" or "Staff"
    public string? Status { get; set; } = "Active"; // "Active" or "Inactive"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
