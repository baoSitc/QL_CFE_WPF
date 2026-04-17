using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("PhieuNhapCT")]
    public class PhieuNhapCT
    {
        [Key]
        public int Id { get; set; }
        public int PhieuNhapId { get; set; }
        public int SanPhamId { get; set; }

        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public SanPham SanPham { get; set; }
    }
}
