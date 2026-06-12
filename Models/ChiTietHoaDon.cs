using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table ("ChiTietHoaDon")]
    public  class ChiTietHoaDon
    {

        public int MaHD { get; set; }
        public int MaSP { get; set; }

        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        [NotMapped]
        public decimal ThanhTien => SoLuong * Gia;

        [ForeignKey(nameof(MaHD))]
        public HoaDon HoaDon { get; set; }

        [ForeignKey("MaSP")]
        public SanPham SanPham { get; set; }
    }
}
