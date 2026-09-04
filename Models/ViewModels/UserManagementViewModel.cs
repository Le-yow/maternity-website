using System.ComponentModel.DataAnnotations;
using MaterniTrack.Models;

namespace MaterniTrack.Models.ViewModels;

public class UserManagementViewModel
{
    public List<UserItemViewModel> Users { get; set; } = new();

    public int TotalUsersCount { get; set; }
    public int AdminUsersCount { get; set; }
    public int StaffUsersCount { get; set; }

    public string CurrentSearch { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = "all";
    public string CurrentStatus { get; set; } = "all";
}

public class UserItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff"; // "Doctor" or "Staff"
    public string Status { get; set; } = "Active"; // "Active" or "Inactive"
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Staff"; // "Doctor" or "Staff"

    [Required]
    public string Status { get; set; } = "Active"; // "Active" or "Inactive"

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Staff";

    [Required]
    public string Status { get; set; } = "Active";

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters if provided.")]
    public string? NewPassword { get; set; }
}
