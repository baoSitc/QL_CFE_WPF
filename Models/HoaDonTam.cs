using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
   public class HoaDonTam
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => Gia * SoLuong;
    }
}
