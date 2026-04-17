using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QL_CFE_WPF.ViewModels
{
    public class SanPhamPosVM
    {
        public int Id { get; set; }
        public string TenSP { get; set; }
        public decimal GiaBan { get; set; }
        public string? HinhAnh { get; set; } // 🔥 thêm dòng này

        public decimal TonKho { get; set; }

        // dùng cho realtime khi chọn
        public decimal TonHienThi => TonKho - SoLuongDangChon;
        public int SoLuongDangChon { get; set; }
        

    }
}
