using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("PhieuNhap")]
    public class PhieuNhap
    {
        [Key]
        public int Id { get; set; }
        public DateTime NgayNhap { get; set; }
        public int NhanVienId { get; set; }
        public decimal TongTien { get; set; }

        public ICollection<PhieuNhapCT> ChiTiet { get; set; }
    }
}
