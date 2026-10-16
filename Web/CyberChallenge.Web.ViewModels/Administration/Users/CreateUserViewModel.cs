using System.ComponentModel.DataAnnotations;

namespace CyberChallenge.Web.ViewModels.Administration.Users;

/// <summary>
/// Данные формы создания аккаунта администратором.
/// </summary>
public class CreateUserViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(User|Administrator)$")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "User";
}