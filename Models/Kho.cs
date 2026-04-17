using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("Kho")]
    public class Kho
    {
        [Key]
        public int Id { get; set; }
        public string TenKho { get; set; }
        public string DiaChi { get; set; }
    }
}
