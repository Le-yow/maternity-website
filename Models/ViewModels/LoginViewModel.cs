using System.ComponentModel.DataAnnotations;

namespace MaterniTrack.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username or email is required")]
    [Display(Name = "Username or Email")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string SelectedRole { get; set; } = "admin"; // "admin" or "staff"

    public string? ReturnUrl { get; set; }
}
