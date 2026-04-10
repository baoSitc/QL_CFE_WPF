using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace QL_CFE_WPF.ViewModels
{
    public partial class ThuTienViewModel:ObservableObject
    {
        public decimal TongTienHienTai { get; set; }

        [ObservableProperty]
        private decimal? tienKhachDua=0;

        [ObservableProperty]
        private decimal tienThoi;

        [ObservableProperty]
        private string phuongThuc;
        
        public bool DuTien => TienKhachDua >= TongTienHienTai;

        public List<string> PhuongThucList { get; } = new()
    {
        "Tiền mặt",
        "Chuyển khoản"
    };

        public Action? OnThanhToanThanhCong;

        partial void OnTienKhachDuaChanged(decimal? value)
        {
            if (value.HasValue)
                TienThoi = value.Value - TongTienHienTai;
            else
                TienThoi = 0;
            OnPropertyChanged(nameof(DuTien)); // 🔥 THÊM DÒNG NÀY
        }
        [RelayCommand]
        void ChonTienNhanh(string soTien)
        {
            if (decimal.TryParse(soTien, out var value))
            {
                TienKhachDua += value;
            }
        }
        [RelayCommand]
        void XacNhanThanhToan()
        {
            if (TienKhachDua < TongTienHienTai)
            {
                MessageBox.Show("Khách đưa chưa đủ tiền!");
                return;
            }

            OnThanhToanThanhCong?.Invoke();
        }
       
    }
}
