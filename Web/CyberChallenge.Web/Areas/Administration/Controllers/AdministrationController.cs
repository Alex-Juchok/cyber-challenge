namespace CyberChallenge.Web.Areas.Administration.Controllers
{
    using CyberChallenge.Common;
    using CyberChallenge.Web.Controllers;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using CyberChallenge.Web.ViewModels.Administration.Users;
    [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
    [Area("Administration")]
    public class AdministrationController : BaseController
    {
    }
}
