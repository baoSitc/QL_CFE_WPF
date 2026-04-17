using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("Permission")]
    public class Permission
    {

        public int Id { get; set; }
        public string MaQuyen { get; set; } // VIEW_REPORT, DELETE_HOADON
        public string TenQuyen { get; set; }
    }
}
