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
        [ForeignKey(nameof(MaBan))]
        public Ban Ban { get; set; }

        public DateTime Ngay { get; set; }
        public string? SoHoaDon { get; set; }

        public decimal TongTien { get; set; }

        public decimal VAT { get; set; } = 0;
        public decimal GiamGia { get; set; } = 0;
        [NotMapped]
        public decimal TienGiamGia => TongTien * (GiamGia / 100);
        [NotMapped]
        public decimal TienVAT => (TongTien-TienGiamGia) * (VAT/100);

        public decimal ThanhTien { get; set; }

        public int TrangThai { get; set; }
        public DateTime GioVao { get; set; } = DateTime.Now;
        public DateTime? GioRa { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public decimal? TienKhachDua { get; set; } = 0;
        public decimal? TienThoi { get; set; } = 0;
        public string? PhuongThuc { get; set; }
        public int? NhanvienID { get; set; }
        [ForeignKey(nameof(NhanvienID))]
        public NhanVien NhanVien { get; set; }

        public int? LanIn { get; set; } = 0;
        public string? NguoiInCuoi { get; set; }
        public DateTime? NgayInCuoi { get; set; }= DateTime.Now;
        // Navigation
        //public Ban Ban { get; set; }
        public List<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new();
        [NotMapped]
        public string TrangThaiText
        {
            get
            {
                return TrangThai switch
                {
                    0 => "Đang phục vụ",
                    1 => "Đã thanh toán",
                    9 => "Đã hủy",
                    _ => ""
                };
            }
        }


    }
}
