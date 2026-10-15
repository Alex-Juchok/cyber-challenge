namespace CyberChallenge.Web.Areas.Administration.Controllers
{
    using CyberChallenge.Common;
    using CyberChallenge.Web.Controllers;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
    [Area("Administration")]
    public class AdministrationController : BaseController
    {
    }
}
