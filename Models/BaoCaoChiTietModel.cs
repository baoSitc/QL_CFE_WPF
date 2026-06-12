using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public class BaoCaoChiTietModel
    {
        public string SoBan { get; set; }
        public DateTime? GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TienGiamGia =>Math.Round( TongTien*GiamGia/100,0);
        public decimal VAT { get; set; }
        public decimal TienVAT => Math.Round((TongTien-TienGiamGia)*VAT/100, 0);
        public decimal TongTien { get; set; }
        public decimal ThanhTien { get; set; }
    }
}
