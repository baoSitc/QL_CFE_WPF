using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("Ban")]
    public class Ban
    {
        [Key]
        public int MaBan { get; set; }
        public string TenBan { get; set; }
        public int TrangThai { get; set; }
        public bool LaPhongVIP { get; set; } = false;
        public int ThuTu { get; set; }

    }
}
