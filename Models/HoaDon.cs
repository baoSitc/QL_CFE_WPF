using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

        public int MaBan { get; set; }

        public DateTime Ngay { get; set; }

        public decimal TongTien { get; set; }

        public decimal VAT { get; set; }

        public decimal GiamGia { get; set; }
        public int TrangThai { get; set; }
        public DateTime GioVao { get; set; } = DateTime.Now;
        public DateTime? GioRa { get; set; }

        // Navigation
        //public Ban Ban { get; set; }
        public List<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new();
    }
}
