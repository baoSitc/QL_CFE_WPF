using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using QL_CFE_WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace QL_CFE_WPF.ViewModels
{
    public partial class PosViewModel : ObservableObject
    {
        //khai báo _khoServices
        
        public ObservableCollection<SanPhamPosVM> SanPhams { get; set; }

        public Dictionary<int, ObservableCollection<CartItem>> GioHangTheoBan
     = new();
        public ObservableCollection<CartItem> GioHang { get; set; } = new ObservableCollection<CartItem>();
        [ObservableProperty]
        private decimal tongTien;
        [ObservableProperty]
        private decimal tongTienHienTai;

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
        private int maBanDangChon;
        [ObservableProperty]
        private string tenBanDangChon;

        [ObservableProperty]
        private decimal giamGia;   // tiền giảm

        [ObservableProperty]
        private decimal phanTramGiam; // % giảm

        [ObservableProperty]
        private decimal vat;   // tiền giảm

        [ObservableProperty]
        private decimal phanTramVAT; // % giảm

        [ObservableProperty]
        private string tuKhoa;
        public ObservableCollection<SanPhamPosVM> SanPhamsLoc =>
    new ObservableCollection<SanPhamPosVM>(
        SanPhams.Where(x => string.IsNullOrEmpty(TuKhoa)
            || x.TenSP.ToLower().Contains(TuKhoa.ToLower()))
    );
        
       
        public PosViewModel()
        {
            LoadSanPham();
            LoadDanhSachBan();
            LoadSauLogin();
        }

        void LoadSauLogin()
        {
            using var db = new AppDbContext();

            var hoaDons = db.HoaDons
                .Where(x => x.TrangThai == 0)
                .ToList();

            foreach (var hd in hoaDons)
            {
                var ct = db.ChiTietHoaDons
                    .Include(x => x.SanPham)
                    .Where(x => x.MaHD == hd.MaHD)
                    .ToList();

                GioHangTheoBan[hd.MaBan] = new ObservableCollection<CartItem>(
                    ct.Select(x => new CartItem
                    {
                        SanPhamId = x.MaSP,
                        SoLuong = x.SoLuong,
                        Gia = x.Gia,
                        TenSP=x.SanPham.TenSP
                    })
                //Load gio hang từ giuban (nếu có), Trạng thái 0 là còn giữ, 1 là đã dùng, 2 là hết hạn


                );
            }
            CapNhatTonHienThi();
        }

        partial void OnTuKhoaChanged(string value)
        {
            OnPropertyChanged(nameof(SanPhamsLoc));
        }
        public List<decimal> DanhSachGiamGia { get; set; } = new()
        {
            0, 5, 10, 20, 30
        };
        void LoadDanhSachBan()
        {
            using var db = new Data.AppDbContext();

            var ds = db.Bans.OrderBy(x=>x.ThuTu).ToList();

            var list = ds.Select(b =>
            {
                // 🔥 ưu tiên giỏ hàng memory
                if (GioHangTheoBan.ContainsKey(b.MaBan))
                {
                    var gio = GioHangTheoBan[b.MaBan];

                    return new BanView
                    {
                        MaBan = b.MaBan,
                        TenBan = b.TenBan,
                        LaPhongVIP = b.LaPhongVIP,
                        DangSuDung = gio.Any(),

                        TongTien = gio.Sum(x => x.SoLuong * x.Gia),
                        SoMon = gio.Sum(x => x.SoLuong),

                        DangChon = (MaBanDangChon == b.MaBan)
                    };
                }
                else
                {
                    // 🔥 fallback DB (chỉ khi chưa load)
                    var hd = db.HoaDons
                        .Include(x => x.ChiTietHoaDons)
                        .Where(x => x.MaBan == b.MaBan && x.TrangThai == 0)
                        .FirstOrDefault();

                    return new BanView
                    {
                        MaBan = b.MaBan,
                        TenBan = b.TenBan,
                        DangSuDung = hd != null,

                        TongTien = hd != null
                            ? hd.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia)
                            : 0,

                        SoMon = hd != null
                            ? hd.ChiTietHoaDons.Sum(x => x.SoLuong)
                            : 0,

                        DangChon = (MaBanDangChon == b.MaBan),
                        GioVao = hd?.GioVao
                    };
                }
            });

            DanhSachBan = new ObservableCollection<BanView>(list);

            //PhanTramGiam = 0;
        }
        //Gop ban
        [RelayCommand]
        void GopBan(BanView banNguon)
        {
            var vm = new ChonBanViewModel(DanhSachBan.ToList());
            var win = new Views.ChonBanWindow
            {
                DataContext = vm
            };
            vm.OnBanSelected = (banDich) =>
            {
                if (banDich.MaBan == banNguon.MaBan) return;
                //Gộp
                GopHaiBan(banNguon, banDich);
                win.Close();
            };
            win.ShowDialog();
        }
        //Chuyen ban
        [RelayCommand]
        void ChuyenBan(BanView banNguon)
        {
            var vm = new ChonBanViewModel(DanhSachBan.ToList());
            var win = new Views.ChonBanWindow
            {
                DataContext = vm
            };
                        vm.OnBanSelected = (banDich) =>
                        { 
                         if (banDich.MaBan == banNguon.MaBan) return;

                            ChuyenBanTrongDB(banNguon, banDich);
                            win.Close();
                        };
            win.ShowDialog();
        }
        void ChuyenBanTrongDB(BanView banNguon, BanView banDich)
        {
            using var db = new Data.AppDbContext();
            var hdNguon = db.HoaDons.FirstOrDefault(x => x.MaBan == banNguon.MaBan && x.TrangThai == 0);
            var hdDich = db.HoaDons.FirstOrDefault(x => x.MaBan == banDich.MaBan && x.TrangThai == 0);
            if (hdNguon != null)
            {
                hdNguon.MaBan = banDich.MaBan;
            }
            if (hdDich != null)
            {
                hdDich.MaBan = banNguon.MaBan;
            }
            db.SaveChanges();
            LoadDanhSachBan();
        }
        void GopHaiBan(BanView banNguon, BanView banDich)
        {
            using var db = new Data.AppDbContext();
            var hdNguon = db.HoaDons.Include(x => x.ChiTietHoaDons).FirstOrDefault(x => x.MaBan == banNguon.MaBan && x.TrangThai == 0);
            var hdDich = db.HoaDons.Include(x => x.ChiTietHoaDons).FirstOrDefault(x => x.MaBan == banDich.MaBan && x.TrangThai == 0);
            if (hdNguon != null && hdDich != null)
            {
                // gộp vào dich
                foreach (var ct in hdNguon.ChiTietHoaDons)
                {
                    var ctDich = hdDich.ChiTietHoaDons.FirstOrDefault(x => x.MaSP == ct.MaSP);
                    if (ctDich != null)
                    {
                        ctDich.SoLuong += ct.SoLuong;
                    }
                    else
                    {
                        hdDich.ChiTietHoaDons.Add(new ChiTietHoaDon
                        {
                            MaHD = hdDich.MaHD,
                            MaSP = ct.MaSP,
                            SoLuong = ct.SoLuong,
                            Gia = ct.Gia
                        });
                    }
                }
                // xóa bàn nguồn
                db.HoaDons.Remove(hdNguon);
            }
            else if (hdNguon != null) // chỉ có bàn nguồn có hóa đơn, chuyển sang bàn đích
            {
                hdNguon.MaBan = banDich.MaBan;
            }
            // nếu chỉ có bàn đích có hóa đơn thì không cần làm gì
            db.SaveChanges();
            LoadDanhSachBan();
        }
        [RelayCommand]
        void ChonBan(object? obj)
        {
            if (obj is not BanView ban) return;

            MaBanDangChon = ban.MaBan;
            TenBanDangChon = ban.TenBan+ (ban.LaPhongVIP ? " (Phòng VIP)" : "");
           

            foreach (var b in DanhSachBan)
                b.DangChon = false;

            ban.DangChon = true;
            using var db = new AppDbContext();

                var hd = db.HoaDons
                    .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                    .FirstOrDefault(x => x.MaBan == ban.MaBan && x.TrangThai == 0);
            if (hd != null) hoaDonHienTai = hd;

            // 🔥 nếu chưa có giỏ → tạo + load DB 1 lần
            if (!GioHangTheoBan.ContainsKey(MaBanDangChon))
            {
                GioHangTheoBan[MaBanDangChon] = new ObservableCollection<CartItem>();

               

                if (hd != null)
                {
                    hoaDonHienTai = hd;                   

                    foreach (var ct in hd.ChiTietHoaDons)
                    {
                        GioHangTheoBan[MaBanDangChon].Add(new CartItem
                        {
                            SanPhamId = ct.MaSP,
                            TenSP = ct.SanPham.TenSP,
                            Gia = ct.Gia,
                            SoLuong = ct.SoLuong
                        });
                    }
                }
                else
                {
                    // hỏi tạo hóa đơn mới
                    if (MessageBox.Show($"Bàn {ban.TenBan} chưa có hóa đơn. Tạo mới?",
                        "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        MaBanDangChon = 0;
                        ban.DangChon = false;
                        return;
                    }

                    var newHd = new HoaDon
                    {
                        MaBan = ban.MaBan,
                        Ngay = DateTime.Now,
                        TrangThai = 0,
                        NhanvienID = Session.CurrentUser.Id
                    };

                    db.HoaDons.Add(newHd);
                    db.SaveChanges();

                    hoaDonHienTai = newHd;
                }
            }
            PhanTramGiam = hd?.GiamGia??0;
            PhanTramVAT = hd?.VAT ?? 0;
            // 🔥 switch giỏ (KHÔNG load lại DB nữa)
            GioHang = GioHangTheoBan[MaBanDangChon];
            OnPropertyChanged(nameof(PhanTramVAT));
            OnPropertyChanged(nameof(PhanTramGiam));

            OnPropertyChanged(nameof(GioHang));
            OnPropertyChanged(nameof(TenBanDangChon));

            CapNhatTonHienThi();

            LoadDanhSachBan();
            CalculateTotal();
        }

        void LoadGioHangTuDB(int banId)
        {
            using var db = new AppDbContext();

            var hd = db.HoaDons
                .Include(x => x.ChiTietHoaDons)
                .ThenInclude(ct => ct.SanPham)
                .FirstOrDefault(x => x.MaBan == banId && x.TrangThai == 0);

            if (hd == null) return;

            foreach (var ct in hd.ChiTietHoaDons)
            {
                GioHangTheoBan[banId].Add(new CartItem
                {
                    SanPhamId = ct.MaSP,
                    TenSP = ct.SanPham.TenSP,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong
                });
            }
        }

        [RelayCommand]
        void TaoBan()
        {
            using var db = new Data.AppDbContext();
            var ban = new Ban { TenBan = $"Bàn {maBanNhap}" };
            //kiem tra xem bàn đã tồn tai chưa
            var hoadonDangMo = db.HoaDons
     .Include(x => x.ChiTietHoaDons)
         .ThenInclude(ct => ct.SanPham)
     .FirstOrDefault(x => x.MaBan == MaBanNhap && x.TrangThai == 0);
            //nếu đã tồn tại bàn đang mở thì không tạo mới
            if (hoadonDangMo != null)
            {
                hoaDonHienTai = hoadonDangMo;
                LoadGioHang(hoadonDangMo);
            }
            else
            {
                //xóa gio hàng cũ nếu có
                GioHang.Clear();
                //Tạo hóa đơn mới cho bàn này
                var hoaDon = new HoaDon
                {
                    MaBan = maBanNhap,
                    Ngay = DateTime.Now,
                    TongTien = 0,
                    TrangThai = 0, // 0: đang mở, 1: đã thanh toán
                    GioVao = DateTime.Now,
                    NhanvienID = Session.CurrentUser.Id // 🔥 gán nhân viên hiện tại vào hóa đơn

                };
                db.HoaDons.Add(hoaDon);
                db.SaveChanges();
                hoaDonHienTai = hoaDon;
                MessageBox.Show($"Bàn {maBanNhap} đã được tạo và sẵn sàng phục vụ.");
            }

        }

        void LoadSanPham()
        {
            using var db = new Data.AppDbContext();

            int khoId = 1; // kho chính

            SanPhams = new ObservableCollection<SanPhamPosVM>(
                (from sp in db.SanPhams
                 join tk in db.TonKhos
                     on sp.MaSP equals tk.SanPhamId into gj
                 from tk in gj.DefaultIfEmpty()
                 where sp.TrangThai == true
                 orderby sp.TenSP
                 select new SanPhamPosVM
                 {
                     Id = sp.MaSP,
                     TenSP = sp.TenSP,
                     GiaBan = sp.Gia,
                     GiaVIP = sp.GiaVIP,
                     HinhAnh = sp.HinhAnh,
                     TonKho = tk != null && tk.KhoId == khoId ? tk.SoLuong : 0
                 }).ToList()
            );
        }

        //thêm sản phẩm vào giỏ hàng
        [RelayCommand]
        void AddToCart(SanPhamPosVM sp)
        {
            if (sp == null) return;
            //KHông cần kiểm tra tồn kho ở đây nữa vì đã kiểm tra khi giữ hàng rồi, nếu không đủ sẽ không cho thêm vào giỏ
            //var giuHangService = new GiuHangService(new AppDbContext());
            ////Giữ hàng trước
            //bool ok=giuHangService.TryReserve(sp.Id, 1, MaBanDangChon, hoaDonHienTai.MaHD);
            //if(!ok)
            //{
            //    MessageBox.Show("Không đủ hàng");
            //    return;
            //}
            // 🔥 2. lưu vào ChiTietHoaDon (QUAN TRỌNG)
            //kiểm tra bàn xem có phải là vip hay không để lấy giá
            using var db = new AppDbContext();

            // lấy thông tin bàn hiện tại
            var ban = db.Bans
                .FirstOrDefault(x => x.MaBan == MaBanDangChon);

            decimal giaBan = sp.GiaBan;

            if (ban?.LaPhongVIP == true)
            {
                giaBan = sp.GiaVIP > 0
                    ? sp.GiaVIP
                    : sp.GiaBan;
            }

          
            var ct = db.ChiTietHoaDons
        .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == sp.Id);

            if (ct != null)
                ct.SoLuong += 1;
            else
            {
                db.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaHD = hoaDonHienTai.MaHD,
                    MaSP = sp.Id,
                    SoLuong = 1,
                    Gia = giaBan                  
                });
            }

            db.SaveChanges();

            //cập nhật giỏ hàng memory
            var itemGioHang = GioHang.FirstOrDefault(x => x.SanPhamId == sp.Id);
            if(itemGioHang != null)
                itemGioHang.SoLuong += 1;
            else
            {
                if (ban?.LaPhongVIP == true)
                {
                    giaBan = sp.GiaVIP > 0
                        ? sp.GiaVIP
                        : sp.GiaBan;
                }
                GioHang.Add(new CartItem
                {
                    SanPhamId = sp.Id,
                    TenSP = sp.TenSP,
                    Gia = giaBan,
                    SoLuong = 1
                });
            }


            

            // 🔥 update tồn hiển thị
           // CapNhatTonHienThi();

            // 🔥 highlight
            //item.IsHighlight = true;

            //Task.Run(async () =>
            //{
            //    await Task.Delay(500);

            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        item.IsHighlight = false;
            //    });
            //});  

            // 🔥 update tổng tiền
            OnPropertyChanged(nameof(TongTien));
            OnPropertyChanged(nameof(TongTienHienTai));
            CalculateTotal();


        }

        private void CapNhatTonHienThi()
        {

            using var db = new AppDbContext();

            var giuService = new GiuHangService(db);

            foreach (var sp in SanPhams)
            {
                var reserved = giuService.GetReserved(sp.Id);

                sp.TonHienThi = sp.TonKho - reserved;  
            }

            OnPropertyChanged(nameof(SanPhamsLoc));

        }

        [RelayCommand]
        public void TangSoLuong(CartItem item)
        {
            var sp = SanPhams.First(x => x.Id == item.SanPhamId);

            //var giuHangService = new GiuHangService(new AppDbContext());
            ////Giữ hàng trước
            //bool ok = giuHangService.TryReserve(sp.Id, 1, MaBanDangChon, hoaDonHienTai.MaHD);
            //if (!ok)
            //{
            //    MessageBox.Show("Không đủ hàng");
            //    return;
            //}
            //kiểm tra bàn xem có phải là vip hay không để lấy giá
            using var db = new AppDbContext();
            var ban = db.Bans
                .FirstOrDefault(x => x.MaBan == MaBanDangChon);

            decimal giaBan = sp.GiaBan;

            if (ban?.LaPhongVIP == true)
            {
                giaBan = sp.GiaVIP > 0
                    ? sp.GiaVIP
                    : sp.GiaBan;
            }

            // 🔥 2. lưu vào ChiTietHoaDon (QUAN TRỌNG)

            var ct = db.ChiTietHoaDons
        .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == sp.Id);

            if (ct != null)
                ct.SoLuong += 1;
            else
            {
                db.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaHD = hoaDonHienTai.MaHD,
                    MaSP = sp.Id,
                    SoLuong = 1,
                    Gia = giaBan
                });
            }

            db.SaveChanges();
            //cập nhật giỏ hàng memory
            var itemGioHang = GioHang.FirstOrDefault(x => x.SanPhamId == sp.Id);
            if (itemGioHang != null)
                itemGioHang.SoLuong += 1;
            else
            {

                GioHang.Add(new CartItem
                {
                    SanPhamId = sp.Id,
                    TenSP = sp.TenSP,
                    Gia = giaBan,
                    SoLuong = 1
                });
            }


            //  item.SoLuong++;
            CapNhatTonHienThi();
            OnPropertyChanged(nameof(TongTien));
            OnPropertyChanged(nameof(TongTienHienTai));
            CalculateTotal();
        }
        [RelayCommand]
        public void GiamSoLuong(CartItem item)
        {
            var giuHangService = new GiuHangService(new AppDbContext());
            using var db = new AppDbContext();
            var ct = db.ChiTietHoaDons
                .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD && x.MaSP == item.SanPhamId);
            if(ct == null) return;


            if (item.SoLuong >1)
            {
                item.SoLuong--;
                ct.SoLuong -= 1;

            }
            else {
                GioHang.Remove(item);
                db.ChiTietHoaDons.Remove(ct);
            }
            giuHangService.Release(item.SanPhamId, hoaDonHienTai.MaHD, 1);

            db.SaveChanges();

            CapNhatTonHienThi();
            OnPropertyChanged(nameof(TongTien));
            OnPropertyChanged(nameof(TongTienHienTai));
            CalculateTotal();
        }
        [RelayCommand]
        void XoaMon(CartItem item)
        {         

            item.SoLuong = 0;
            if (item.SoLuong <= 0)
                GioHang.Remove(item);
            CapNhatTonHienThi();
            OnPropertyChanged(nameof(TongTien));
            OnPropertyChanged(nameof(TongTienHienTai));
            CalculateTotal();

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
            PhanTramGiam = 0; // reset giảm giá khi thay đổi món
        }
        partial void OnPhanTramGiamChanged(decimal value)

        {
            if (GioHang == null || GioHang.Count == 0) return;
            var tong = hoaDonHienTai.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia);
            GiamGia =Math.Round( tong * value / 100,0);

            // Cập nhật gIAM GIA vào database
            using var db = new AppDbContext();
            var hd = db.HoaDons.FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD);
            if (hd != null)
                hd.GiamGia = PhanTramGiam;
            db.SaveChanges();
            CalculateTotal();
        }
        //tính VAT
        partial void OnPhanTramVATChanged(decimal value)
        {
            if (GioHang == null || GioHang.Count == 0) return;
            var tong = hoaDonHienTai.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia);
            Vat =Math.Round( (tong - GiamGia) * value / 100,0);
            // Cập nhật VAT vào database
            using var db = new AppDbContext();
            var hd = db.HoaDons.FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD);
            if (hd != null)
                hd.VAT = PhanTramVAT;
            db.SaveChanges();
            
            CalculateTotal();
        }

        void TinhTienSauGiamGia()
        {
            if (hoaDonHienTai == null) return;
            var tong = hoaDonHienTai.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia);
            if (PhanTramGiam > 0)
            {
                GiamGia = tong * PhanTramGiam / 100;
            }

        }
        //load gio hang
        void LoadGioHang(HoaDon hd)
        {
            GioHang.Clear();

            foreach (var ct in hd.ChiTietHoaDons)
            {
                GioHang.Add(new CartItem
                {
                    SanPhamId = ct.MaSP,
                    TenSP = ct.SanPham.TenSP,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong
                });
            }
          

            CapNhatTonHienThi();
            CalculateTotal();
        }
        void CalculateTotal()
        {
            TinhTienSauGiamGia();
            TongTien = GioHang.Sum(h => h.Gia * h.SoLuong);         

            GiamGia =Math.Round(TongTien * PhanTramGiam/100,0);
            Vat = Math.Round((TongTien - GiamGia) * PhanTramVAT / 100, 0);
            TongTienHienTai =Math.Round( TongTien + Vat - GiamGia,0);
            var ban = DanhSachBan.FirstOrDefault(x => x.MaBan == MaBanDangChon);
            if (ban != null)
            {
                ban.TongTien = TongTien; // 🔥 UI tự update
                //update số món
                ban.SoMon = GioHang.Sum(x => x.SoLuong);
            }
        }
        //xóa sản phẩm khỏi giỏ hàng
        [RelayCommand]
        void RemoveFromCart(CartItem item)
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
            //Thanh toán 
            if(hoaDonHienTai == null) return;
            var vm = new ThuTienViewModel
            {
                TongTienHienTai = this.TongTienHienTai,
                PhuongThuc = "Tiền mặt"
            };
            var win = new Views.ThuTienWindow
            {
                DataContext = vm
            };
            vm.OnThanhToanThanhCong = () =>
            {
                var hoaDon = db.HoaDons             
            .Include(x => x.ChiTietHoaDons)
            .ThenInclude(ct => ct.SanPham)
            .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD);
                // 🔥 SYNC giỏ hàng → DB
                db.ChiTietHoaDons.RemoveRange(hoaDon.ChiTietHoaDons);
                foreach (var item in GioHang)
                {
                    db.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        MaHD = hoaDon.MaHD,
                        MaSP = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        Gia = item.Gia
                    });
                }

                
                hoaDon.TrangThai = 1; // đã thanh toán
                hoaDon.NgayThanhToan = DateTime.Now;
                hoaDon.TienKhachDua = vm.TienKhachDua;
                hoaDon.TienThoi = vm.TienThoi;
                hoaDon.PhuongThuc = vm.PhuongThuc;            

                hoaDon.GioRa = DateTime.Now;
            hoaDon.TongTien =Math.Round(TongTien,0);
                hoaDon.ThanhTien= Math.Round(TongTienHienTai, 0);
                hoaDon.GiamGia = phanTramGiam;
            hoaDon.VAT = phanTramVAT;
                hoaDon.LanIn++;
                hoaDon.NgayInCuoi = DateTime.Now;
                hoaDon.NguoiInCuoi = Session.CurrentUser.TenDangNhap;
                //tìm số hóa đơn
                if (string.IsNullOrEmpty(hoaDon.SoHoaDon))
                {
                    var hoaDonService =
                        new HoaDonService(db);

                    hoaDon.SoHoaDon =
                        hoaDonService.TaoSoHoaDon();
                }
                db.SaveChanges();


            TongTien = 0;
                db.Entry(hoaDon).Collection(x => x.ChiTietHoaDons).Load();

                var hoaDonIn = db.HoaDons
                    .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                    .First(x => x.MaHD == hoaDon.MaHD);


                InBill(hoaDonIn);
                //tính hàng xuất kho
                // 🔥 xuất kho (cùng context)
                //var service = new KhoService(db);

                //service.XuatKho(
                //    GioHang.Select(x => new XuatKhoItem
                //    {
                //        SanPhamId = x.SanPhamId,
                //        SoLuong = x.SoLuong
                //    }).ToList(),
                //    Session.CurrentUser.Id,
                //    1,
                //    "BAN_HANG"
                //);

                //var giu=db.GiuHangs.Where(x => x.MaBan == MaBanDangChon 
                //&& x.HoaDonId == hoaDon.MaHD && x.TrangThai == 0).ToList();

                //foreach (var g in giu)
                //{
                //    g.TrangThai = 1; // đã dùng
                //}
                //db.SaveChanges();


                // reset UI
                GioHangTheoBan.Remove(MaBanDangChon);
                GioHang.Clear();
                hoaDonHienTai = null;
            MaBanDangChon = 0;
            TongTienHienTai = 0;
            GiamGia = 0;
            LoadDanhSachBan();
                LoadSanPham();
                win.Close();
            };
            win.ShowDialog();
        }

        void InBill(HoaDon hd)
        {
            pdfBillService pdf= new pdfBillService();
            pdf.ExportPdf(hd);
            return;
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true) return;

            var doc = new FlowDocument();

            // 🔥 khổ giấy (58mm / 80mm)
            doc.PageWidth = 300;
            doc.PagePadding = new Thickness(10);

            // 🔥 font chuẩn bill
            doc.FontFamily = new FontFamily("Courier New");
            doc.FontSize = 12;

            // ================= HEADER =================
            doc.Blocks.Add(new Paragraph(new Run("QUÁN CAFE BẰNG LĂNG TÍM"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run("-----------------------------"))
            {
                TextAlignment = TextAlignment.Center
            });
            // No65i dung so ban
            var sobanTable = new Table();
            sobanTable.Columns.Add(new TableColumn { Width = new GridLength(120) });
            sobanTable.Columns.Add(new TableColumn { Width = new GridLength(140) });

            var sobanlGroup = new TableRowGroup();
            sobanTable.RowGroups.Add(sobanlGroup);

            var sobanRow = new TableRow();

            sobanRow.Cells.Add(new TableCell(new Paragraph(new Run($"Bàn: {hd.MaBan}"))));

            sobanRow.Cells.Add(new TableCell(new Paragraph(new Run($"Giờ Vào:{hd.GioVao.ToString("HH:mm")}")))
            {
                TextAlignment = TextAlignment.Right,

            });

            sobanlGroup.Rows.Add(sobanRow);
            var row2 = new TableRow();

            row2.Cells.Add(new TableCell(
                new Paragraph(new Run($"Thu ngân: Yến Vy"))
            ));

            row2.Cells.Add(new TableCell(
                new Paragraph(new Run($"Giờ ra: {hd.GioRa?.ToString("HH:mm") ?? ""}"))
            )
            {
                TextAlignment = TextAlignment.Right
            });

            sobanlGroup.Rows.Add(row2);

            doc.Blocks.Add(sobanTable);

            //doc.Blocks.Add(new Paragraph(new Run($"Bàn: {hd.MaBan}    {DateTime.Now:HH:mm}")));

            // ================= TABLE =================
            var table = new Table();

            // 3 cột
            table.Columns.Add(new TableColumn { Width = new GridLength(140) }); // tên
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });  // ĐG
            table.Columns.Add(new TableColumn { Width = new GridLength(20) });  // SL
            table.Columns.Add(new TableColumn { Width = new GridLength(60) });  // tiền

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            // tiêu đề bảng
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Tên hàng"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("ĐG")))
            {
                TextAlignment = TextAlignment.Center
            });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("SL")))
            {
                TextAlignment = TextAlignment.Center
            });
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("T.Tiền")))
            {
                TextAlignment = TextAlignment.Center
            });
            // in đậm tiêu đề
            foreach (var cell in headerRow.Cells)
            {
                cell.FontWeight = FontWeights.Bold;
                cell.Padding = new Thickness(2);
            }
            rowGroup.Rows.Add(headerRow);
            // dòng kẻ
            rowGroup.Rows.Add(new TableRow()); // spacer

            // ===== DATA =====
            foreach (var ct in hd.ChiTietHoaDons)
            {
                var row = new TableRow();

                // tên (cho phép xuống dòng)
                row.Cells.Add(new TableCell(new Paragraph(new Run(ct.SanPham?.TenSP ?? "")))
                {
                    Padding = new Thickness(2),
                });
                // đơn giá
                row.Cells.Add(new TableCell(new Paragraph(new Run(ct.Gia.ToString("N0"))))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(2)
                });

                // số lượng
                row.Cells.Add(new TableCell(new Paragraph(new Run(ct.SoLuong.ToString("N0"))))
                {
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(2)
                });

                // tiền
                row.Cells.Add(new TableCell(new Paragraph(
                    new Run((ct.SoLuong * ct.Gia).ToString("N0"))))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(2)
                });

                rowGroup.Rows.Add(row);
            }


            doc.Blocks.Add(table);

            // ================= FOOTER =================
            doc.Blocks.Add(new Paragraph(new Run("-----------------------------"))
            {
                TextAlignment = TextAlignment.Center
            });

            // tổng tiền
            var totalTable = new Table();
            totalTable.Columns.Add(new TableColumn { Width = new GridLength(190) });
            totalTable.Columns.Add(new TableColumn { Width = new GridLength(80) });

            var totalGroup = new TableRowGroup();
            totalTable.RowGroups.Add(totalGroup);

            var totalRow = new TableRow();

            totalRow.Cells.Add(new TableCell(new Paragraph(new Run("TỔNG CỘNG")))
            {
                FontWeight = FontWeights.Bold
            });

            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(hd.TongTien.ToString("N0"))))
            {
                TextAlignment = TextAlignment.Right,
                FontWeight = FontWeights.Bold
            });

            totalGroup.Rows.Add(totalRow);

            doc.Blocks.Add(totalTable);

            // lời cảm ơn
            doc.Blocks.Add(new Paragraph(new Run("Cảm ơn quý khách!"))
            {
                TextAlignment = TextAlignment.Center
            });

            // ================= PRINT =================
            printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Bill");
        }
       
        
    }
}
