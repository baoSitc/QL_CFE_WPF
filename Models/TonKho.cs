using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("TonKho")]
    public class TonKho
    {
        [Key]
        public int SanPhamId { get; set; }
        public int KhoId { get; set; }
        public decimal SoLuong { get; set; }

        //public SanPham SanPham { get; set; }
    }
}
