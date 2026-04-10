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
using System.Windows.Media;

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
        private int? maBanDangChon;

        [ObservableProperty]
        private decimal giamGia;   // tiền giảm

        [ObservableProperty]
        private int phanTramGiam; // % giảm
        [ObservableProperty]
        private string tuKhoa;
        public ObservableCollection<SanPham> SanPhamsLoc =>
    new ObservableCollection<SanPham>(
        SanPhams.Where(x => string.IsNullOrEmpty(TuKhoa)
            || x.TenSP.ToLower().Contains(TuKhoa.ToLower()))
    );
        public PosViewModel()
        {
            LoadSanPham();
            LoadDanhSachBan();
        }
        partial void OnTuKhoaChanged(string value)
        {
            OnPropertyChanged(nameof(SanPhamsLoc));
        }
        public List<int> DanhSachGiamGia { get; set; } = new()
        {
            0, 5, 10, 20, 30
        };
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
                    DangSuDung = hd != null,
                    // 🔥 tính realtime
                    TongTien = hd != null ? hd.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia)
            : 0,
                    // 🔥 QUAN TRỌNG
                    DangChon = (maBanDangChon == b.MaBan),
                    GioVao = hd?.GioVao,
                    SoMon = hd != null ? hd.ChiTietHoaDons.Sum(x => x.SoLuong) : 0
                };
            });

            DanhSachBan = new ObservableCollection<BanView>(list);
            PhanTramGiam = 0;
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
            // 🔥 lưu lại bàn đang chọn
            MaBanDangChon = ban.MaBan;
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
                    MaBanDangChon = null;
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
                hoaDonHienTai = hoaDon;
                MessageBox.Show($"Bàn {maBanNhap} đã được tạo và sẵn sàng phục vụ.");
            }

        }

        void LoadSanPham()
        {
            using var db = new Data.AppDbContext();
            SanPhams = new ObservableCollection<SanPham>(db.SanPhams.OrderBy(x => x.TenSP).ToList());
        }

        //thêm sản phẩm vào giỏ hàng
        [RelayCommand]
        void AddToCart(SanPham sp)
        {
            if (sp == null || hoaDonHienTai == null) return;

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
                    Gia = sp.Gia,

                });

            }

            db.SaveChanges();

            // reload lại UI
            var hd = db.HoaDons
                .Include(x => x.ChiTietHoaDons)
                    .ThenInclude(ct => ct.SanPham)
                .First(x => x.MaHD == hoaDonHienTai.MaHD);

            hoaDonHienTai = hd;
            LoadGioHang(hd);
            var item = GioHang.FirstOrDefault(x => x.MaSP == sp.MaSP);

            if (item != null)
            {
                item.IsHighlight = true;

                // 🔥 tắt sau 300ms
                Task.Run(async () =>
                {
                    await Task.Delay(600);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        item.IsHighlight = false;
                    });
                });
            }


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
            PhanTramGiam = 0; // reset giảm giá khi thay đổi món
        }
        partial void OnPhanTramGiamChanged(int value)

        {
            if (GioHang == null || GioHang.Count == 0) return;
            var tong = hoaDonHienTai.ChiTietHoaDons.Sum(x => x.SoLuong * x.Gia);
            GiamGia = tong * value / 100;

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
            using var db = new Data.AppDbContext();

            gioHang.Clear();

            foreach (var ct in hd.ChiTietHoaDons.OrderBy(x => x.SanPham.TenSP))
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
            TinhTienSauGiamGia();
            TongTien = GioHang.Sum(h => h.Gia * h.SoLuong);
            TongTienHienTai = TongTien - GiamGia;
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
       .Include(x => x.ChiTietHoaDons).ThenInclude(ct => ct.SanPham)
       .FirstOrDefault(x => x.MaHD == hoaDonHienTai.MaHD);
                hoaDon.TrangThai = 1; // đã thanh toán
                hoaDon.NgayThanhToan = DateTime.Now;
                hoaDon.TienKhachDua = vm.TienKhachDua;
                hoaDon.TienThoi = vm.TienThoi;
                hoaDon.PhuongThuc = vm.PhuongThuc;               

                hoaDon.GioRa = DateTime.Now;
            hoaDon.TongTien = TongTienHienTai;
            hoaDon.GiamGia = GiamGia;
            db.SaveChanges();
            TongTien = 0;
            InBill(hoaDon);

            // reset UI
            GioHang.Clear();
            hoaDonHienTai = null;
            MaBanDangChon = null;
            TongTienHienTai = 0;
            GiamGia = 0;
            LoadDanhSachBan();
                win.Close();
            };
            win.ShowDialog();
        }

        void InBill(HoaDon hd)
        {
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
