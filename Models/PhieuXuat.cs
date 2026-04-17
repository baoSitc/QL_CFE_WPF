using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("PhieuXuat")]
    public class PhieuXuat
    {
        [Key]
        public int Id { get; set; }
        public DateTime NgayXuat { get; set; }
        public int NhanVienId { get; set; }
        public string LoaiXuat { get; set; }

        public ICollection<PhieuXuatCT> ChiTiet { get; set; }
    }
}
