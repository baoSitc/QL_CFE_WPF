using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public partial class CartItem : ObservableObject,INotifyPropertyChanged
    {
        public int SanPhamId { get; set; }
        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        //animation
        [ObservableProperty]
        private bool isHighlight;


        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal ThanhTien => SoLuong * Gia;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
