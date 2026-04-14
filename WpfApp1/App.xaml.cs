using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{

    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUserFullName { get; set; }
        public static int CurrentUserRoleId { get; set; }
        public static string CurrentUserRole { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CurrentUserId = 0;
            CurrentUserFullName = "Гость";
            CurrentUserRoleId = 1;
            CurrentUserRole = "Guest";
        }
    }


}
