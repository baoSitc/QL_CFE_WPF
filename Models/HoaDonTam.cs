using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public partial class HoaDonTam : ObservableObject
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => Gia * SoLuong;
        //animation
        [ObservableProperty]
        private bool isHighlight ;
       
       
    }
}
