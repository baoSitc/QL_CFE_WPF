using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace QL_CFE_WPF.ViewModels
{
    public partial class BaoCaoViewModel : ObservableObject
    {
        public ObservableCollection<BaoCaoChiTietModel> BaoCaoChiTiet { get; set; } = new();

        public DateTime TuNgay { get; set; } = DateTime.Today;
        public DateTime DenNgay { get; set; } = DateTime.Today;
        [ObservableProperty]
                private decimal tongDoanhThu;

        [RelayCommand]
        void LoadBaoCaoChiTiet()
        {
            using var db = new Data.AppDbContext();

            var data = db.HoaDons
                .Include(x =>x.Ban)
                .Where(x => x.TrangThai == 1 &&
                            x.Ngay >= TuNgay &&
                            x.Ngay <= DenNgay.AddDays(1).AddTicks(-1))
                .Select(x => new BaoCaoChiTietModel
                {
                    SoBan = x.Ban.TenBan,
                    GioVao = x.GioVao,
                    GioRa = x.GioRa,
                    GiamGia = x.GiamGia,
                    VAT = x.VAT,
                    TongTien = x.TongTien,
                    ThanhTien = x.ThanhTien
                })
                .OrderByDescending(x => x.GioRa)
                .ToList();
            TongDoanhThu = data.Sum(x => x.ThanhTien);
            BaoCaoChiTiet.Clear();
            foreach (var item in data)
                BaoCaoChiTiet.Add(item);
        }
        [RelayCommand]
        void InBaoCao()
        {
            // 🔥 tạo document
            FlowDocument doc = TaoDocumentBaoCao();

            PrintDialog pd = new PrintDialog();

            if (pd.ShowDialog() == true)
            {
               

                // 🔥 set A4
                doc.PageWidth = 793;
                doc.PageHeight = 1122;

                // 🔥 lề
                doc.PagePadding = new Thickness(40);

                // 🔥 chia trang tự động
                doc.ColumnWidth = double.PositiveInfinity;

                pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Bao cao A4");
            }
        }
        FlowDocument TaoDocumentBaoCao()
        {
            var doc = new FlowDocument();
            // 🔥 THÔNG TIN QUÁN
            doc.Blocks.Add(new Paragraph(new Run("QUÁN CAFE BẰNG LĂNG TÍM"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            // 🔥 TIÊU ĐỀ
            doc.Blocks.Add(new Paragraph(new Run("BÁO CÁO DOANH THU"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            doc.Blocks.Add(new Paragraph(new Run(
                $"Từ ngày: {TuNgay:dd/MM/yyyy} - Đến ngày: {DenNgay:dd/MM/yyyy}"))
            {
                TextAlignment = TextAlignment.Center
            });

            // 🔥 TABLE
            Table table = new Table();

            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            // HEADER
            var headerGroup = new TableRowGroup();
            var headerRow = new TableRow();

            headerRow.Cells.Add(Cell("Bàn", true));
            headerRow.Cells.Add(Cell("Giờ vào", true));
            headerRow.Cells.Add(Cell("Giờ ra", true));
            headerRow.Cells.Add(Cell("Giảm", true));
            headerRow.Cells.Add(Cell("VAT", true));
            headerRow.Cells.Add(Cell("Tổng", true));

            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // DATA
            var dataGroup = new TableRowGroup();

            foreach (var item in BaoCaoChiTiet)
            {
                var row = new TableRow();

                row.Cells.Add(Cell(item.SoBan));
                row.Cells.Add(Cell(item.GioVao?.ToString("HH:mm")));
                row.Cells.Add(Cell(item.GioRa?.ToString("HH:mm")));
                row.Cells.Add(Cell(item.TienGiamGia.ToString("N0")));
                row.Cells.Add(Cell(item.TienVAT.ToString("N0")));
                row.Cells.Add(Cell(item.ThanhTien.ToString("N0")));

                dataGroup.Rows.Add(row);
            }

            table.RowGroups.Add(dataGroup);

            doc.Blocks.Add(table);
            

            // 🔥 TỔNG
            doc.Blocks.Add(new Paragraph(new Run($"TỔNG: {TongDoanhThu:N0} đ"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right
            });
            //Tên quán
            

            return doc;
        }
        TableCell Cell(string text, bool isHeader = false)
        {
            return new TableCell(new Paragraph(new Run(text ?? "")))
            {
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                Padding = new Thickness(5),
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(0.5),
                TextAlignment = isHeader ? TextAlignment.Center : TextAlignment.Right

            };
        }
        [RelayCommand]
        void ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel (*.xlsx)|*.xlsx";
            dlg.FileName = $"BaoCao_{DateTime.Now:yyyyMMdd}.xlsx";

            if (dlg.ShowDialog() != true) return;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("BaoCao");

            int row = 1;
            // 🔥 THÔNG TIN QUÁN
            ws.Cells[row, 1].Value = "QUÁN CAFE BẰNG LĂNG TÍM";
            ws.Cells[row, 1, row, 6].Merge = true;
            ws.Cells[row, 1].Style.Font.Size = 16;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            row++;
            // 🔥 TIÊU ĐỀ
            ws.Cells[row, 1].Value = "BÁO CÁO DOANH THU";
            ws.Cells[row, 1, row, 6].Merge = true;
            ws.Cells[row, 1].Style.Font.Size = 16;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row++;

            ws.Cells[row, 1].Value = $"Từ ngày: {TuNgay:dd/MM/yyyy} - Đến ngày: {DenNgay:dd/MM/yyyy}";
            ws.Cells[row, 1, row, 6].Merge = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row += 2;

            // 🔥 HEADER
            ws.Cells[row, 1].Value = "Số bàn";
            ws.Cells[row, 2].Value = "Giờ vào";
            ws.Cells[row, 3].Value = "Giờ ra";
            ws.Cells[row, 4].Value = "Giảm giá";
            ws.Cells[row, 5].Value = "VAT";
            ws.Cells[row, 6].Value = "Tổng tiền";

            using (var range = ws.Cells[row, 1, row, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            row++;

            // 🔥 DATA
            foreach (var item in BaoCaoChiTiet)
            {
                ws.Cells[row, 1].Value = item.SoBan;
                ws.Cells[row, 2].Value = item.GioVao?.ToString("HH:mm");
                ws.Cells[row, 3].Value = item.GioRa?.ToString("HH:mm");
                ws.Cells[row, 4].Value = item.TienGiamGia;
                ws.Cells[row, 5].Value = item.TienVAT;
                ws.Cells[row, 6].Value = item.ThanhTien;

                // format tiền
                ws.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // 🔥 TỔNG
            ws.Cells[row, 5].Value = "TỔNG:";
            ws.Cells[row, 5].Style.Font.Bold = true;

            ws.Cells[row, 6].Value = TongDoanhThu;
            ws.Cells[row, 6].Style.Font.Bold = true;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";

            // 🔥 AUTO WIDTH
            ws.Cells.AutoFitColumns();

            // 🔥 BORDER
            using (var range = ws.Cells[3, 1, row, 6])
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
    }
}
