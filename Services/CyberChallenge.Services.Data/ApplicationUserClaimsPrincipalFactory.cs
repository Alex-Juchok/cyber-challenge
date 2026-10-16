using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using CyberChallenge.Data.Models;
using CyberChallenge.Common;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly UserManager<ApplicationUser> userManager;

    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        this.userManager = userManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        // Проверяем, есть ли у пользователя роль, если нет — добавляем
        var roles = await this.userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            await this.userManager.AddToRoleAsync(user, GlobalConstants.UserRoleName);
        }

        return await base.GenerateClaimsAsync(user);
    }
}
