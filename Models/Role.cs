using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("Role")]
    public class Role
    {
       
        public int Id { get; set; }
        public string TenRole { get; set; } // Admin, Thu ngân...
    }
}
