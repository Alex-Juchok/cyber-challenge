using System.ComponentModel.DataAnnotations;

namespace CyberChallenge.Web.ViewModels.Administration.Users;

/// <summary>
/// Данные формы изменения существующего аккаунта.
/// </summary>
public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(User|Administrator)$")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "User";
}