using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;



namespace QL_CFE_WPF.ViewModels
{
    public partial class PosViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<SanPham> sanPhams;
        [ObservableProperty]
        private ObservableCollection<HoaDonTam> gioHang = new();
        [ObservableProperty]
        private decimal tongTien;
        [ObservableProperty]
        private int maBanNhap;
        private HoaDon hoaDonHienTai;
        //[ObservableProperty]
        //private ObservableCollection<Ban> danhSachBan=new();
        [ObservableProperty]
        private Ban selectedBan;
        [ObservableProperty]
        private ObservableCollection<BanView> danhSachBan = new();
        [ObservableProperty]
        private int? maBanDangChon;

        public PosViewModel()
        {
            LoadSanPham();
            LoadDanhSachBan();
        }
        void LoadDanhSachBan()
        {
            using var db = new Data.AppDbContext();

            var ds = db.Bans.ToList();

            var list = ds.Select(b =>
            {
                var hd = db.HoaDons.Include(x => x.ChiTietHoaDons) // eager loading chi tiết hóa đơn để tính tổng tiền
                    .Where(x => x.MaBan == b.MaBan && x.TrangThai == 0) //Trạng thái 0 là đang mở,1 la đã thanh toán
                    .FirstOrDefault();

                return new BanView
                {
                    MaBan = b.MaBan,
                    TenBan = b.TenBan,
                    DangSuDung= hd != null,
                    // 🔥 tính realtime
                    TongTien = hd != null? hd.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia)
            : 0,
                    // 🔥 QUAN TRỌNG
                    DangChon = (maBanDangChon == b.MaBan)
                };
            });

            DanhSachBan = new ObservableCollection<BanView>(list);
        }
        [RelayCommand]
        void ChonBan(object? obj)
        {
            if (obj is not BanView ban) return;
            // 🔥 lưu lại bàn đang chọn
            maBanDangChon = ban.MaBan;
            // reset chọn
            foreach (var b in DanhSachBan)
                b.DangChon = false;

            ban.DangChon = true;

            using var db = new Data.AppDbContext();

            var hd = db.HoaDons
                .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefault(x => x.MaBan == ban.MaBan && x.TrangThai == 0);//Trạng thái 0 là đang mở,1 la đã thanh toán

            if (hd != null)
            {
                hoaDonHienTai = hd;
                LoadGioHang(hd);
            }
            else
            {
                //xác định bàn này chưa có hóa đơn nào đang mở, tạo mới
                //hỏi khách hàng con muốn tạo hóa đơn mới cho bàn này không
                if (MessageBox.Show($"Bàn {ban.TenBan} hiện chưa có hóa đơn nào. Bạn có muốn tạo mới?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    //nếu không tạo mới thì reset lại bàn đang chọn
                    maBanDangChon = null;
                    ban.DangChon = false;
                   
                    return;
                }

                var newHd = new HoaDon
                {
                    MaBan = ban.MaBan,
                    Ngay = DateTime.Now,
                    TrangThai = 0 // 0: đang mở, 1: đã thanh toán
                };

                db.HoaDons.Add(newHd);
                db.SaveChanges();

                hoaDonHienTai = newHd;
                GioHang.Clear();
            }

            LoadDanhSachBan(); // refresh lại tiền + màu
        }

        [RelayCommand]
        void TaoBan()
        {
           using var db = new Data.AppDbContext();
            var ban = new Ban { TenBan = $"Bàn {maBanNhap}" };
            //kiem tra xem bàn đã tồn tại chưa
            var hoadonDangMo = db.HoaDons
     .Include(x => x.ChiTietHoaDons)
         .ThenInclude(ct => ct.SanPham)
     .FirstOrDefault(x => x.MaBan == MaBanNhap && x.TrangThai == 0);
            //nếu đã tồn tại bàn đang mở thì không tạo mới
            if (hoadonDangMo != null)
            {
                hoaDonHienTai=hoadonDangMo;
                LoadGioHang(hoadonDangMo);
            }
            else
            {
                //xóa gio hàng cũ nếu có
                gioHang.Clear();
                //Tạo hóa đơn mới cho bàn này
                var hoaDon = new HoaDon
                {
                    MaBan = maBanNhap,
                    Ngay = DateTime.Now,
                    TongTien = 0,
                    TrangThai = 0, // 0: đang mở, 1: đã thanh toán
                    GioVao = DateTime.Now
                };
                db.HoaDons.Add(hoaDon);
                db.SaveChanges();
                hoaDonHienTai=hoaDon;
                MessageBox.Show($"Bàn {maBanNhap} đã được tạo và sẵn sàng phục vụ.");
            }
           
        }

        void LoadSanPham()
        {
            using var db = new Data.AppDbContext();
            SanPhams = new ObservableCollection<SanPham>(db.SanPhams.OrderBy(x=>x.TenSP).ToList());
        }

        //thêm sản phẩm vào giỏ hàng
        [RelayCommand]
        void AddToCart(SanPham sp)
        {
            if (sp == null || hoaDonHienTai==null) return;

            using var db = new Data.AppDbContext();

            // kiểm tra đã có món chưa
            var ct = db.ChiTietHoaDons
                .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == sp.MaSP);

            if (ct != null)
            {
                ct.SoLuong += 1;
            }
            else
            {
                db.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaHD = hoaDonHienTai.MaHD,
                    MaSP = sp.MaSP,
                    SoLuong = 1,
                    Gia = sp.Gia
                });
            }

            db.SaveChanges();

            // reload lại UI
            var hd = db.HoaDons
                .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                .First(x => x.MaHD == hoaDonHienTai.MaHD);

            hoaDonHienTai = hd;
           
           
        }
        [RelayCommand]
        void TangSoLuong(object? obj)
        {
            if (obj is not HoaDonTam item) return;

            using var db = new Data.AppDbContext();

            var ct = db.ChiTietHoaDons
                .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == item.MaSP);

            if (ct != null)
            {
                ct.SoLuong += 1;
                db.SaveChanges();
            }

            LoadLaiHoaDon();
        }
        [RelayCommand]
        void GiamSoLuong(object? obj)
        {
            if (obj is not HoaDonTam item) return;

            using var db = new Data.AppDbContext();

            var ct = db.ChiTietHoaDons
                .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == item.MaSP);

            if (ct != null)
            {
                ct.SoLuong -= 1;

                if (ct.SoLuong <= 0)
                    db.ChiTietHoaDons.Remove(ct);

                db.SaveChanges();
            }

            LoadLaiHoaDon();
        }
        [RelayCommand]
        void XoaMon(object? obj)
        {
            if (obj is not HoaDonTam item) return;

            using var db = new Data.AppDbContext();

            var ct = db.ChiTietHoaDons
                .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == item.MaSP);

            if (ct != null)
            {
                db.ChiTietHoaDons.Remove(ct);
                db.SaveChanges();
            }

            LoadLaiHoaDon();
        }
        void LoadLaiHoaDon()
        {
            using var db = new Data.AppDbContext();

            var hd = db.HoaDons
                .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                .First(x => x.MaHD == hoaDonHienTai.MaHD);

            hoaDonHienTai = hd;
            LoadGioHang(hd);
        }

        //load gio hang
        void LoadGioHang(HoaDon hd)
        {
            using var db = new Data.AppDbContext();

            gioHang.Clear();

            foreach (var ct in hd.ChiTietHoaDons.OrderBy(x=>x.SanPham.TenSP))
            {
                // var sp = db.SanPhams.Find(ct.MaSP);

                gioHang.Add(new HoaDonTam
                {
                    MaSP = ct.MaSP,
                    TenSP = ct.SanPham?.TenSP,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong                   

                });
            }
           

            CalculateTotal();
        }
        void CalculateTotal()
        {
            TongTien = GioHang.Sum(h => h.Gia * h.SoLuong);
            var ban = DanhSachBan.FirstOrDefault(x => x.MaBan == MaBanDangChon);
            if (ban != null)
            {
                ban.TongTien = TongTien; // 🔥 UI tự update
            }
        }
        //xóa sản phẩm khỏi giỏ hàng
        [RelayCommand]
        void RemoveFromCart(HoaDonTam item)
        {
            if (item == null) return;
            GioHang.Remove(item);
            CalculateTotal();
        }
        //thanh toán
        [RelayCommand]
        void ThanhToan()
        {
            // Xử lý thanh toán ở đây (lưu hóa đơn vào database, in hóa đơn, v.v.)
            // Sau khi thanh toán xong, xóa giỏ hàng và cập nhật tổng tiền
            using var db = new Data.AppDbContext();
            //var hoaDon = db.HoaDons.FirstOrDefault(h => h.MaBan == maBanDangChon && h.TrangThai == 0);
            var hoaDon = db.HoaDons
       .Include(x => x.ChiTietHoaDons).ThenInclude(ct => ct.SanPham)
       .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD);

            hoaDon.TrangThai = 1; // đánh dấu đã thanh toán
            hoaDon.GioRa = DateTime.Now;
            hoaDon.TongTien = TongTien;
            db.SaveChanges();
           
            TongTien = 0;
            InBill(hoaDon);

            // reset UI
            GioHang.Clear();
            hoaDonHienTai = null;
            MaBanDangChon = null;
            LoadDanhSachBan();
        }
        void InBill(HoaDon hd)
        {
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                var doc = new FlowDocument();

                doc.Blocks.Add(new Paragraph(new Run("QUÁN CAFE BẰNG LĂNG TÍM"))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                });

                doc.Blocks.Add(new Paragraph(new Run($"Bàn: {hd.MaBan}")));
                doc.Blocks.Add(new Paragraph(new Run($"Giờ vào: {hd.GioVao:HH:mm}")));
                doc.Blocks.Add(new Paragraph(new Run($"Giờ ra: {hd.GioRa:HH:mm}")));

                doc.Blocks.Add(new Paragraph(new Run("--------------------------------")));

                foreach (var ct in hd.ChiTietHoaDons)
                {
                    doc.Blocks.Add(new Paragraph(
                        new Run($"{ct.SanPham?.TenSP} x{ct.SoLuong} = {(ct.SoLuong * ct.Gia):N0} đ")));
                }

                doc.Blocks.Add(new Paragraph(new Run("--------------------------------")));

                doc.Blocks.Add(new Paragraph(new Run($"TỔNG: {hd.TongTien:N0} đ"))
                {
                    FontWeight = FontWeights.Bold
                });

                doc.PagePadding = new Thickness(20);

                printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "In hóa đơn");
            }
        }
    }
}
