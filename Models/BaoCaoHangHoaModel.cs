using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public class BaoCaoHangHoaModel
    {
        public int Stt { get; set; }=0;
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public DateTime? Ngay { get; set; }
        public string SoBan { get; set; }
        public string? TenNhom    { get; set; }
    }
}
