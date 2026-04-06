using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public partial class BanView : ObservableObject
    {
        public int MaBan { get; set; }
        public string TenBan { get; set; }

        [ObservableProperty]
        private bool dangSuDung;

        [ObservableProperty]
        private decimal tongTien;

        [ObservableProperty]
        private bool dangChon;
    }
}
