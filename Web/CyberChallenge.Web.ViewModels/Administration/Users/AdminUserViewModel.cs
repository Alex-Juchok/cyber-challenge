namespace CyberChallenge.Web.ViewModels.Administration.Users
{
    /// <summary>
    /// Модель представления строки пользователя в административной панели.
    /// </summary>
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsBlocked { get; set; }
    }
}