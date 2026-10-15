using System;
using System.Collections.Generic;
using CyberChallenge.Web.ViewModels.Administration.Users;

namespace CyberChallenge.Web.ViewModels.Administration.Dashboard
{
    public class IndexViewModel
    {
        public int SettingsCount { get; set; }

        // Список пользователей для административной панели.
        public IReadOnlyCollection<AdminUserViewModel> Users { get; set; }
            = Array.Empty<AdminUserViewModel>();
    }
}
