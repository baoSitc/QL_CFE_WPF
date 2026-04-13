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
        public decimal VAT { get; set; }
        public decimal TongTien { get; set; }
        public decimal ThanhTien => TongTien + GiamGia;
    }
}
