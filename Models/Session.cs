using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public static class Session
    {
        public static NhanVien CurrentUser { get; set; }
        public static List<string> Permissions { get; set; } = new();
    }
}
