using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public class TopSanPhamModel
    {
        public string TenSP { get; set; } = "";
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
        public string? HinhAnh { get; set; }  // dùng lại ImageConverter của ông
    }
}
