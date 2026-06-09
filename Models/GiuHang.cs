using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("GiuHang")]
    public class GiuHang
    {
        public int Id { get; set; }

        public int SanPhamId { get; set; }
        public decimal SoLuong { get; set; }

        public int MaBan { get; set; }
        public int HoaDonId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }

        public int TrangThai { get; set; } // 0: giữ, 1: đã dùng, 2: hết hạn
    }
}
