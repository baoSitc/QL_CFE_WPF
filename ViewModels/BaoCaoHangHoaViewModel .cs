using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;

namespace QL_CFE_WPF.ViewModels
{
    public partial class BaoCaoHangHoaViewModel : ObservableObject
    {
        public ObservableCollection<BaoCaoHangHoaModel> BaoCaoHangHoa { get; set; } = new();

        public List<KieuBaoCao> DanhSachKieu { get; } =
            Enum.GetValues(typeof(KieuBaoCao)).Cast<KieuBaoCao>().ToList();

        [ObservableProperty]
        private KieuBaoCao kieuDangChon = KieuBaoCao.ChiTiet;

        [ObservableProperty]
        private DateTime tuNgay = DateTime.Today;

        [ObservableProperty]
        private DateTime denNgay = DateTime.Today;

        public decimal TongTien => BaoCaoHangHoa.Sum(x => x.ThanhTien);
        

        [RelayCommand]
        void LoadBaoCaoHangHoa()
        {
            using var db = new Data.AppDbContext();

            IQueryable<ChiTietHoaDon> query = db.ChiTietHoaDons
                
                .Where(ct => ct.HoaDon.TrangThai == 1 &&
                             ct.HoaDon.Ngay >= TuNgay &&
                             ct.HoaDon.Ngay <= DenNgay.AddDays(1).AddTicks(-1));

            List<BaoCaoHangHoaModel> data;

            switch (KieuDangChon)
            {
                case KieuBaoCao.ChiTiet:
                    data = query
                        .Select(ct => new BaoCaoHangHoaModel
                    {
                        TenSP = ct.SanPham.TenSP,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.Gia,
                        ThanhTien = ct.SoLuong * ct.Gia,
                        Ngay = ct.HoaDon.Ngay,
                        SoBan = ct.HoaDon.MaBan.ToString(),
                        TenNhom = ct.SanPham.NhomHang.TenNhom,
                    })
                        .OrderBy(ct => ct.TenNhom)                     
                    .ThenByDescending(x => x.Ngay)
                    .ToList();
                    break;

                case KieuBaoCao.TongHop:
                    data = query
                        .GroupBy(ct => ct.SanPham.TenSP)
                        .Select(g => new BaoCaoHangHoaModel
                        {
                            TenSP = g.Key,
                            SoLuong = g.Sum(x => x.SoLuong),
                            DonGia = g.First().Gia,
                            ThanhTien = g.Sum(x => x.SoLuong * x.Gia),
                            TenNhom = g.First().SanPham.NhomHang.TenNhom

                        })                        
                        .OrderByDescending(x => x.TenNhom)
                        .ToList();
                    break;

                case KieuBaoCao.TopBanChay:
                    data = query
                        .GroupBy(ct => ct.SanPham.TenSP)
                        .Select(g => new BaoCaoHangHoaModel
                        {
                            TenSP = g.Key,
                            SoLuong = g.Sum(x => x.SoLuong),
                            ThanhTien = g.Sum(x => x.SoLuong * x.Gia)
                        })
                        .OrderByDescending(x => x.SoLuong)
                        .Take(5)
                        .ToList();
                    break;

                default:
                    data = new List<BaoCaoHangHoaModel>();
                    break;
            }

            BaoCaoHangHoa.Clear();
            int stt = 0;
            foreach (var item in data)
            {
                item.Stt = ++stt;
                BaoCaoHangHoa.Add(item);
            }

            OnPropertyChanged(nameof(TongTien));
        }
        [RelayCommand]
        void ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel (*.xlsx)|*.xlsx";
            dlg.FileName = $"BaoCaoHangBan_{DateTime.Now:yyyyMMdd}.xlsx";
            LoadBaoCaoHangHoa();

            if (dlg.ShowDialog() != true) return;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("BaoCaoHangBan");

            int row = 1;
            // 🔥 THÔNG TIN QUÁN
            ws.Cells[row, 1].Value = "PHỞ CÔ 9 GIA LAI";
            ws.Cells[row, 1, row, 5].Merge = true;
            ws.Cells[row, 1].Style.Font.Size = 16;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row++;
            // 🔥 TIÊU ĐỀ
            ws.Cells[row, 1].Value = "BÁO CÁO HÀNG BÁN";
            ws.Cells[row, 1, row, 5].Merge = true;
            ws.Cells[row, 1].Style.Font.Size = 16;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row++;

            ws.Cells[row, 1].Value = $"Từ ngày: {TuNgay:dd/MM/yyyy} - Đến ngày: {DenNgay:dd/MM/yyyy}";
            ws.Cells[row, 1, row, 5].Merge = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row += 2;

            // 🔥 HEADER
            ws.Cells[row, 1].Value = "Stt";
            ws.Cells[row, 2].Value = "Tên Hàng";
            ws.Cells[row, 3].Value = "Số Lượng";
            ws.Cells[row, 4].Value = "Đơn Giá";
            ws.Cells[row, 5].Value = "Thành Tiền";           


            using (var range = ws.Cells[row, 1, row, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            row++;

            // 🔥 DATA
            foreach (var item in BaoCaoHangHoa)
            {
                ws.Cells[row, 1].Value = item.Stt;
                ws.Cells[row, 2].Value = item.TenSP;
                ws.Cells[row, 3].Value = item.SoLuong;
                ws.Cells[row, 4].Value = item.DonGia;
                ws.Cells[row, 5].Value = item.ThanhTien;
                row++;
            }   
               

                // format tiền
                ws.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0";

                row++;
            

            // 🔥 TỔNG
            ws.Cells[row, 4].Value = "TỔNG:";
            ws.Cells[row, 4].Style.Font.Bold = true;

            ws.Cells[row, 5].Value = TongTien;
            ws.Cells[row, 5].Style.Font.Bold = true;
            ws.Cells[row, 5].Style.Numberformat.Format = "#,##0";

            // 🔥 AUTO WIDTH
            ws.Cells.AutoFitColumns();

            // 🔥 BORDER
            using (var range = ws.Cells[5, 1, row, 5])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // SAVE FILE
            File.WriteAllBytes(dlg.FileName, package.GetAsByteArray());
            MessageBox.Show("Xuất Excel thành công!");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dlg.FileName,
                UseShellExecute = true
            });

        }
        //iN BÁO CÁO
        [RelayCommand]
        void InBaoCao()
        {
            LoadBaoCaoHangHoa();
            if (BaoCaoHangHoa == null || BaoCaoHangHoa.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu để in", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 🔥 tạo document
            FlowDocument doc = TaoDocumentBaoCao();

            PrintDialog pd = new PrintDialog();

            if (pd.ShowDialog() == true)
            {


                // 🔥 set A4
                doc.PageWidth = 793;
                doc.PageHeight = 1122;

                // 🔥 lề
                doc.PagePadding = new Thickness(0);

                // 🔥 chia trang tự động
                doc.ColumnWidth = double.PositiveInfinity;

                pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Bao cao A4");
            }
        }
        FlowDocument TaoDocumentBaoCao()
        {
            var doc = new FlowDocument();
            // 🔥 THÔNG TIN QUÁN
            doc.Blocks.Add(new Paragraph(new Run("PHỞ CÔ 9 GIA LAI"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman")
            });

            // 🔥 TIÊU ĐỀ
            doc.Blocks.Add(new Paragraph(new Run( KieuDangChon==KieuBaoCao.ChiTiet?"BÁO CÁO CHI TIẾT HÀNG HÓA":"BÁO CÁO TỔNG HỢP HÀNG HÓA"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman")
            });

            doc.Blocks.Add(new Paragraph(new Run(
                $"Từ ngày: {TuNgay:dd/MM/yyyy} - Đến ngày: {DenNgay:dd/MM/yyyy}"))
            {
                TextAlignment = TextAlignment.Center,
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman")
            });

            // 🔥 TABLE
            Table table = new Table();
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            

            // HEADER
            var headerGroup = new TableRowGroup();
            var headerRow = new TableRow();
         

            headerRow.Cells.Add(Cell("Stt", true));
            headerRow.Cells.Add(Cell("Tên Hàng", true));
            headerRow.Cells.Add(Cell("Số Lượng", true));
            headerRow.Cells.Add(Cell("Đơn Giá", true));
            headerRow.Cells.Add(Cell("Thành Tiền", true));

            

            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // DATA
            var dataGroup = new TableRowGroup();

            foreach (var item in BaoCaoHangHoa)
            {
                var row = new TableRow();

                row.Cells.Add(Cell(item.Stt.ToString()));
                row.Cells.Add(Cell(item.TenSP.ToString()));
                row.Cells.Add(Cell(item.SoLuong.ToString("N0")));
                row.Cells.Add(Cell(item.DonGia.ToString("N0")));
                row.Cells.Add(Cell(item.ThanhTien.ToString("N0")));
                

                dataGroup.Rows.Add(row);
            }

            table.RowGroups.Add(dataGroup);
            table.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");

            doc.Blocks.Add(table);


            // 🔥 TỔNG
            doc.Blocks.Add(new Paragraph(new Run($"TỔNG: {TongTien:N0} đ"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right,
                FontFamily = new System.Windows.Media.FontFamily("Times New Roman")
            });
            //Tên quán


            return doc;
        }
        TableCell Cell(string text, bool isHeader = false)
        {
            return new TableCell(new Paragraph(new Run(text ?? "")))
            {
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                Padding = new Thickness(0),
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0.5),
                TextAlignment = isHeader ? TextAlignment.Center : TextAlignment.Right

            };
        }
    }
}
