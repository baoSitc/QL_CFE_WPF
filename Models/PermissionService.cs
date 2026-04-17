using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public static class PermissionService
    {
        public static bool Has(string key)
        {
            return Session.Permissions.Contains(key);
        }
    }
}
