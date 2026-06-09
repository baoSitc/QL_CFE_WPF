using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int MaSP { get; set; }

        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        public string? HinhAnh { get; set; } // 🔥 thêm dòng này
        public bool TrangThai { get; set; } // 0: ngừng kinh doanh, 1: đang kinh doanh
        public int? NhomHangId { get; set; }

        public virtual NhomHang NhomHang { get; set; }
    }
}
