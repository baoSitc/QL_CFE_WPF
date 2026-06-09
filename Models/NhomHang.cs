using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QL_CFE_WPF.Models
{
    [Table("NhomHang")]
    public class NhomHang
    {
        public int Id { get; set; }

        public string TenNhom { get; set; }

        public int? ParentId { get; set; }

        public virtual NhomHang Parent { get; set; }

        public virtual ICollection<NhomHang> Children { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
