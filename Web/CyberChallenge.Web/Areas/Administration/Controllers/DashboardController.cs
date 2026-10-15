namespace CyberChallenge.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using CyberChallenge.Services.Data;
    using CyberChallenge.Services.Data.Users;
    using CyberChallenge.Web.ViewModels.Administration.Dashboard;
    using CyberChallenge.Web.ViewModels.Administration.Users;
    using Microsoft.AspNetCore.Mvc;

    public class DashboardController : AdministrationController
    {
        private readonly ISettingsService settingsService;
        private readonly IUserManagementService userManagementService;

        public DashboardController(
            ISettingsService settingsService,
            IUserManagementService userManagementService)
        {
            this.settingsService = settingsService;
            this.userManagementService = userManagementService;
        }

        /// <summary>
        /// Отображает панель администратора и список пользователей.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var users = await this.userManagementService.GetAllAsync();

            var viewModel = new IndexViewModel
            {
                SettingsCount = this.settingsService.GetCount(),

                Users = users.Select(user => new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Role = user.Role,
                    IsBlocked = user.IsBlocked,
                }).ToList(),
            };

            return this.View(viewModel);
        }

        /// <summary>
        /// Блокирует пользователя и возвращает администратора на Dashboard.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var currentAdminId =
                this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentAdminId))
            {
                return this.Forbid();
            }

            var result = await this.userManagementService.BlockAsync(
                userId,
                currentAdminId);

            if (result.Succeeded)
            {
                this.TempData["SuccessMessage"] = "Пользователь заблокирован.";
            }
            else
            {
                this.TempData["ErrorMessage"] =
                    string.Join(" ", result.Errors.Select(error => error.Description));
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Снимает постоянную блокировку пользователя.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var result = await this.userManagementService.UnblockAsync(userId);

            if (result.Succeeded)
            {
                this.TempData["SuccessMessage"] =
                    "Блокировка пользователя снята.";
            }
            else
            {
                this.TempData["ErrorMessage"] =
                    string.Join(" ", result.Errors.Select(error => error.Description));
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
