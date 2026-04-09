using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace QL_CFE_WPF.ViewModels
{
    public partial class ChonBanViewModel : ObservableObject
    {
        public ObservableCollection<BanView> DanhSachBan { get; set; }

        public Action<BanView> OnBanSelected;

        public ChonBanViewModel(List<BanView> ds)
        {
            DanhSachBan = new ObservableCollection<BanView>(ds);
        }

        [RelayCommand]
        void ChonBan(BanView ban)
        {
            OnBanSelected?.Invoke(ban);
        }
    }
}
