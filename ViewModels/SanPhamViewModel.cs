using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QL_CFE_WPF.ViewModels
{
    public partial class SanPhamViewModel : ObservableObject
    {
        [ObservableProperty]
        private string hinhAnh;

        [ObservableProperty]
        private BitmapImage anhPreview;
        [RelayCommand]
        private void ChonAnh()
        {
            var dlg = new OpenFileDialog();

            dlg.Filter =
                "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (dlg.ShowDialog() != true)
                return;

            string sourceFile = dlg.FileName;

            string imageFolder =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images");

            if (!Directory.Exists(imageFolder))
                Directory.CreateDirectory(imageFolder);

            string fileName =
                Guid.NewGuid().ToString()
                + Path.GetExtension(sourceFile);

            string destFile =
                Path.Combine(imageFolder, fileName);

            File.Copy(sourceFile,
                      destFile,
                      true);

            HinhAnh = @"Images\" + fileName;

            AnhPreview = new BitmapImage(
                new Uri(destFile));
        }


        [ObservableProperty]        
        private ObservableCollection<SanPham> sanPhams;
        public ObservableCollection<NhomHang> NhomHangs { get; set; }

        private NhomHang _selectedNhomHang;
        public NhomHang SelectedNhomHang
        {
            get => _selectedNhomHang;
            set
            {
                _selectedNhomHang = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<NhomHangTreeVM> _rootNhomHangs;
        //Phần treeview
        public ObservableCollection<NhomHangTreeVM> RootNhomHangs
        {
            get => _rootNhomHangs;
            set
            {
                _rootNhomHangs = value;
                OnPropertyChanged();
            }
        }
        public void LoadNhomHang()
        {
            using var db = new AppDbContext();

            var ds = db.NhomHangs.ToList();

            RootNhomHangs = new ObservableCollection<NhomHangTreeVM>(
                BuildTree(ds, 0)
            );
        }
        //crud nhóm hàng
        [ObservableProperty]
        private bool isAddingNhom=false;
        [ObservableProperty]
        private bool isEditingNhom=false;

        [ObservableProperty]
        private string tenNhomMoi;
        //thêm nhóm hàng mới
        [RelayCommand]
        private void AddNhom()
        {
            //kiểm tra phải là thêm nhóm hàng cấp 1 hay cấp con
            using var db = new AppDbContext();

            TenNhomMoi = "";
            IsAddingNhom = true;
        }
        //Edit nhóm hàng
        //thêm nhóm hàng mới
        [RelayCommand]
        private void EditNhom()
        {
            //kiểm tra phải là thêm nhóm hàng cấp 1 hay cấp con
            using var db = new AppDbContext();

            TenNhomMoi = SelectedNhomHang?.TenNhom ?? "";
            IsAddingNhom = true;
            isEditingNhom = true;
        }
        [RelayCommand]
        private void CancelAddNhom()
        {
            IsAddingNhom = false;
        }
        //save nhóm hàng mới
        [RelayCommand]
        private void SaveNhom()
        {
            if (string.IsNullOrWhiteSpace(TenNhomMoi))
                return;


           using var db = new AppDbContext();
            //Kiểm tra là Edit hay Add
            if (isAddingNhom && !isEditingNhom)
            {
                //kiểm tra tên nhóm hàng đã tồn tại chưa
                var exists = db.NhomHangs.Any(x => x.TenNhom == TenNhomMoi);
                if (exists)
                {
                    // Hiển thị thông báo lỗi
                    MessageBox.Show("Tên nhóm hàng đã tồn tại. Vui lòng chọn tên khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                db.NhomHangs.Add(new NhomHang
                {
                    TenNhom = TenNhomMoi,

                    ParentId = SelectedNhomHangId
                });

               
            }
            else if (isEditingNhom)
            {
                var nhom = db.NhomHangs.Find(SelectedNhomHang.Id);
                if (nhom != null)
                {
                    nhom.TenNhom = TenNhomMoi;
                }
            } 

            db.SaveChanges();

            LoadNhomHang();

            IsAddingNhom = false;
        }
        private List<NhomHangTreeVM> BuildTree(
    List<NhomHang> all,
    int? parentId)
        {
            return all
                .Where(x => x.ParentId == parentId)
                .Select(x => new NhomHangTreeVM
                {
                    Id = x.Id,
                    TenNhom = x.TenNhom,

                    Children = new ObservableCollection<NhomHangTreeVM>(
                        BuildTree(all, x.Id)
                    )
                })
                .ToList();
        }
        //khai báo từ khóa để lọc sản phẩm theo nhóm hàng
        [ObservableProperty]
        private string tuKhoa;

        partial void OnTuKhoaChanged(string value)
        {
            LoadSanPhamTheoNhom();
        }

        public int SelectedNhomHangId { get; set; }
        public void LoadSanPhamTheoNhom()
        {
            using var db = new AppDbContext();
            if (SelectedNhomHangId > 0)
                SanPhams = new ObservableCollection<SanPham>(
                    db.SanPhams.Include(x => x.NhomHang)
                      .Where(x => x.NhomHangId == SelectedNhomHangId
                      && (string.IsNullOrEmpty(tuKhoa) || x.TenSP.Contains(tuKhoa))
                      )
                      .OrderBy(x => x.TenSP)
                      .ToList()
                );
            else
            {
                SanPhams = new ObservableCollection<SanPham>(
                db.SanPhams.Include(x => x.NhomHang)
                  .Where(x=>x.TenSP.Contains(TuKhoa) )                  
                  .OrderBy(x => x.TenSP)
                  .ToList()
            );
                
            }

        }

        //Constructor
        public SanPhamViewModel()
        {
            LoadNhomHang();
            LoadData();
        }

        void LoadData()
        {

            using var db = new Data.AppDbContext();
            NhomHangs = new ObservableCollection<NhomHang>(
    db.NhomHangs
      .OrderBy(x => x.TenNhom)
      .ToList());

            SanPhams = new ObservableCollection<SanPham>(db.SanPhams.Include(x => x.NhomHang)
                .OrderBy(x => x.TenSP).ToList());

        }
        [RelayCommand]
        void AddSanPham()
        {
            var newSanPham = new SanPham
            {
                TenSP = "Chưa có",
                Gia = Gia,
                GiaVIP = GiaVIP,
                NhomHangId = SelectedNhomHangId,
                TrangThai = true
            };
            using var db = new Data.AppDbContext();
            db.SanPhams.Add(newSanPham);
            db.SaveChanges();
            SanPhams.Add(newSanPham);
        }
        [RelayCommand]
        void RemoveSanPham(SanPham sp)
        {
            if (sp == null) return;
            using var db = new Data.AppDbContext();
            db.SanPhams.Remove(sp);
            db.SaveChanges();
            SanPhams.Remove(sp);
        }
       
        public List<DonViTinh> DanhSachDonViTinh { get; } =
              Enum.GetValues(typeof(DonViTinh)).Cast<DonViTinh>().ToList();
        [ObservableProperty]
        private DonViTinh donViTinhDangChon = DonViTinh.To;



        [ObservableProperty]
        private string tenSP;
        [ObservableProperty]
        private decimal gia;
        [ObservableProperty]
        private decimal giaVIP;        
        public bool TrangThai { get; set; }

        [ObservableProperty]
        private SanPham selectedSanPham;
        [RelayCommand]
        void UpdateSanPham()
        {
            if (SelectedSanPham == null) return;
            using var db = new Data.AppDbContext();
            var sp = db.SanPhams.Find(SelectedSanPham.MaSP);
            if (sp != null)
            {
                sp.TenSP = SelectedSanPham.TenSP;
                sp.Gia = SelectedSanPham.Gia;
                sp.GiaVIP = SelectedSanPham.GiaVIP;
                sp.NhomHangId = SelectedNhomHangId;
                sp.TrangThai = SelectedSanPham.TrangThai;
                sp.HinhAnh = HinhAnh;
                sp.DonViTinh = SelectedSanPham.DonViTinh;
                db.SaveChanges();
                LoadSanPhamTheoNhom();
                //LoadData();
               
               
            }


        }

        partial void OnSelectedSanPhamChanged(SanPham value)
        {
            if (value != null)
            {
                
                TenSP = value.TenSP;
                Gia = value.Gia;
                GiaVIP = value.GiaVIP;
                HinhAnh = value.HinhAnh??"";
                SelectedNhomHangId = value.NhomHangId??0;
            

                string fullPath =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        value.HinhAnh ?? "");

                if (File.Exists(fullPath))
                {
                    AnhPreview = new BitmapImage(
                        new Uri(fullPath));
                }
                //xóa trắng ảnh nếu không có HinhAnh
                else
                {
                    AnhPreview = new BitmapImage(
                        new Uri(
                            Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                "Images",
                                "no-image.png")));
                }

            }
        }
        //load icon cho treeview
       
    }
}
