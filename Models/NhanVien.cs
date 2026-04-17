using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int Id { get; set; }

        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }

        public string TenHienThi { get; set; }

        public int RoleId { get; set; } // 🔥 thay vì string VaiTro
        public Role Role { get; set; }

        public bool TrangThai { get; set; } // true = đang làm
    }
}
