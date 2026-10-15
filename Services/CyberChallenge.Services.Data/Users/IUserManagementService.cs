using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CyberChallenge.Services.Data.Users
{
    /// <summary>
    /// Описывает операции административного управления пользователями.
    /// </summary>
    public interface IUserManagementService
    {
        Task<IReadOnlyCollection<UserManagementDto>> GetAllAsync();

        Task<IdentityResult> BlockAsync(
            string userId,
            string currentAdminId);

        Task<IdentityResult> UnblockAsync(
            string userId);
    }
}