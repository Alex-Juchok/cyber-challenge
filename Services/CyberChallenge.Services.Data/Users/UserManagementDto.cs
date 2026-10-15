namespace CyberChallenge.Services.Data.Users
{
    /// <summary>
    /// Данные пользователя, доступные для отображения администратору.
    /// Не содержит пароля и других секретных данных.
    /// </summary>
    public class UserManagementDto
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsBlocked { get; set; }
    }
}