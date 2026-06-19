using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace QL_CFE_WPF.ViewModels
{
    public partial class BanViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Ban> bans = new();
        [ObservableProperty]
        private Ban selectedBan;
        


        [ObservableProperty]
        private string tenBan;

        [ObservableProperty]
        private bool laPhongVIP;

        [ObservableProperty]
        private int trangThai = 1;

        public ObservableCollection<ComboItem> TrangThaiList
            = new()
            {
            new() { Value = 1, Name = "Hoạt động" },
            new() { Value = 0, Name = "Ngưng sử dụng" }
            };
        [RelayCommand]
        private void Save()
        {
            using var db = new AppDbContext();

            foreach (var ban in Bans)
            {
                if (ban.MaBan == 0)
                    db.Bans.Add(ban);
                else
                    db.Bans.Update(ban);
            }

            db.SaveChanges();
            LoadBan();

            MessageBox.Show("Đã lưu");
        }
        //Contructor
        public BanViewModel() 
        {
            LoadBan();
            
        }
        void LoadBan()
        {
            var db = new AppDbContext();
            Bans = new ObservableCollection<Ban>(
                db.Bans.OrderBy(x => x.ThuTu).ToList());
        }
        [RelayCommand]
        private void AddBan()
        {
            Bans.Add(new Ban
            {
                TenBan = "Bàn mới",
                LaPhongVIP = false,
                TrangThai = 1,
                ThuTu=Bans.Count()+1
            });
        }
        [RelayCommand]
        private void DeleteBan()
        {
            if (SelectedBan == null)
                return;

            if (MessageBox.Show(
                $"Xóa {SelectedBan.TenBan} ?",
                "Xác nhận",
                MessageBoxButton.YesNo)
                != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();

            var ban = db.Bans.Find(SelectedBan.MaBan);

            if (ban != null)
            {
                db.Bans.Remove(ban);
                db.SaveChanges();
            }

            Bans.Remove(SelectedBan);
        }
        [RelayCommand]
        private void MoveUp()
        {
            if (SelectedBan == null)
                return;

            var current = SelectedBan;

            var prev = Bans
                .Where(x => x.ThuTu < current.ThuTu)
                .OrderByDescending(x => x.ThuTu)
                .FirstOrDefault();

            if (prev == null)
                return;

            int temp = current.ThuTu;
            current.ThuTu = prev.ThuTu;
            prev.ThuTu = temp;

            RefreshGrid();
        }
        [RelayCommand]
        private void MoveDown()
        {
            if (SelectedBan == null)
                return;

            var current = SelectedBan;

            var next = Bans
                .Where(x => x.ThuTu > current.ThuTu)
                .OrderBy(x => x.ThuTu)
                .FirstOrDefault();

            if (next == null)
                return;

            int temp = current.ThuTu;
            current.ThuTu = next.ThuTu;
            next.ThuTu = temp;

            RefreshGrid();
        }
        private void RefreshGrid()
        {
            Bans = new ObservableCollection<Ban>(
                Bans.OrderBy(x => x.ThuTu));
        }



    }
}
