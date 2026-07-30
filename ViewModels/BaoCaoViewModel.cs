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
                    Ngay = x.Ngay,
                    SoBan = x.Ban.TenBan,
                    GioVao = x.GioVao,
                    GioRa = x.GioRa,
                    GiamGia = x.GiamGia,
                    VAT = x.VAT,
                    TongTien = x.TongTien,
                    ThanhTien = x.ThanhTien,
                    PhuongThuc = x.PhuongThuc?? ""

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
               

                //// 🔥 set A4 đứng
                //doc.PageWidth = 793;
                //doc.PageHeight = 1122;
                // A4 Landscape
                doc.PageWidth = 1122;   // ngang
                doc.PageHeight = 793;   // dọc

                // 🔥 lề
                doc.PagePadding = new Thickness(40);

                // 🔥 chia trang tự động
                doc.ColumnWidth = double.PositiveInfinity;
                // Thiết lập Landscape cho máy in
                if (pd.PrintTicket != null)
                {
                    pd.PrintTicket.PageOrientation =
                        System.Printing.PageOrientation.Landscape;
                }

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
                TextAlignment = TextAlignment.Center,FontFamily= new System.Windows.Media.FontFamily("Times New Roman")
            });

            // 🔥 TIÊU ĐỀ
            doc.Blocks.Add(new Paragraph(new Run("BÁO CÁO DOANH THU"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,FontFamily= new System.Windows.Media.FontFamily("Times New Roman")
            });

            doc.Blocks.Add(new Paragraph(new Run(
                $"Từ ngày: {TuNgay:dd/MM/yyyy} - Đến ngày: {DenNgay:dd/MM/yyyy}"))
            {
                TextAlignment = TextAlignment.Center,FontFamily= new System.Windows.Media.FontFamily("Times New Roman")
            });

            // 🔥 TABLE
            Table table = new Table();

            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            // HEADER
            var headerGroup = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(Cell("Ngày", true));
            headerRow.Cells.Add(Cell("Bàn", true));
            headerRow.Cells.Add(Cell("Giờ vào", true));
            headerRow.Cells.Add(Cell("Giờ ra", true));
            headerRow.Cells.Add(Cell("Tổng cộng", true));
            headerRow.Cells.Add(Cell("%Giảm", true));
            headerRow.Cells.Add(Cell("Tiền giảm", true));
            headerRow.Cells.Add(Cell("%VAT", true));
            headerRow.Cells.Add(Cell("Tiền VAT", true));
            headerRow.Cells.Add(Cell("Thành tiền", true));
            headerRow.Cells.Add(Cell("Phương thức thanh toán", true));

            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // DATA
            var dataGroup = new TableRowGroup();

            foreach (var item in BaoCaoChiTiet)
            {
                var row = new TableRow();
                row.Cells.Add(Cell(item.Ngay?.ToString("dd/MM/yyyy")));
                row.Cells.Add(Cell(item.SoBan));
                row.Cells.Add(Cell(item.GioVao?.ToString("HH:mm")));
                row.Cells.Add(Cell(item.GioRa?.ToString("HH:mm")));
                row.Cells.Add(Cell(item.TongTien.ToString("N0")));
                row.Cells.Add(Cell(item.GiamGia.ToString("N0")+"%"));
                row.Cells.Add(Cell(item.TienGiamGia.ToString("N0")));
                row.Cells.Add(Cell(item.VAT.ToString("N0")+"%"));
                row.Cells.Add(Cell(item.TienVAT.ToString("N0")));
                row.Cells.Add(Cell(item.ThanhTien.ToString("N0")));
                row.Cells.Add(Cell(item.PhuongThuc ?? "")); 

                dataGroup.Rows.Add(row);
            }

            table.RowGroups.Add(dataGroup);
            table.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");

            doc.Blocks.Add(table);


            // 🔥 TỔNG
            doc.Blocks.Add(new Paragraph(new Run($"TỔNG: {TongDoanhThu:N0} đ"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right,FontFamily = new System.Windows.Media.FontFamily("Times New Roman")
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
            ws.Cells[row, 1].Value = "PHỞ CÔ 9 GIA LAI";
            ws.Cells[row, 1, row, 11].Merge = true;
            ws.Cells[row, 1].Style.Font.Size = 16;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            row++;
            // 🔥 TIÊU ĐỀ
            ws.Cells[row, 1].Value = "BÁO CÁO DOANH THU";
            ws.Cells[row, 1, row, 11].Merge = true;
            ws.Cells[row, 1].Style.Font.Size = 16;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row++;

            ws.Cells[row, 1].Value = $"Từ ngày: {TuNgay:dd/MM/yyyy} - Đến ngày: {DenNgay:dd/MM/yyyy}";
            ws.Cells[row, 1, row, 11].Merge = true;
            ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row += 2;

            // 🔥 HEADER
            ws.Cells[row, 1].Value = "Ngày";
            ws.Cells[row, 2].Value = "Số bàn";
            ws.Cells[row, 3].Value = "Giờ vào";
            ws.Cells[row, 4].Value = "Giờ ra";
            ws.Cells[row, 5].Value = "Tổng cộng";
            ws.Cells[row, 6].Value = "%Giảm";
            ws.Cells[row, 7].Value = "Tiền giảm";
            ws.Cells[row, 8].Value = "%VAT";
            ws.Cells[row, 9].Value = "Tiền VAT";
            ws.Cells[row, 10].Value = "Thành tiền";
            ws.Cells[row, 11].Value = "Phương thức thanh toán";






            using (var range = ws.Cells[row, 1, row, 11])
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
                    ws.Cells[row, 1].Value = item.Ngay?.ToString("dd/MM/yyyy");
                    ws.Cells[row, 2].Value = item.SoBan;
                    ws.Cells[row, 3].Value = item.GioVao?.ToString("HH:mm");
                    ws.Cells[row, 4].Value = item.GioRa?.ToString("HH:mm");
                    ws.Cells[row, 5].Value = item.TongTien;
                    ws.Cells[row, 6].Value = item.GiamGia;
                    ws.Cells[row, 7].Value = item.TienGiamGia;
                ws.Cells[row, 8].Value = item.VAT;
                ws.Cells[row, 9].Value = item.TienVAT;
                ws.Cells[row, 10].Value = item.ThanhTien;
                ws.Cells[row, 11].Value = item.PhuongThuc;

                // format tiền
                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 7].Style.Numberformat.Format = "#,##0";
                ws.Cells[row,8].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                ws.Cells[row, 10].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // 🔥 TỔNG
            ws.Cells[row, 9].Value = "TỔNG:";
            ws.Cells[row, 9].Style.Font.Bold = true;
            ws.Cells[row, 9].Style.Font.Size = 14;

            ws.Cells[row, 10].Value = TongDoanhThu;
            ws.Cells[row, 10].Style.Font.Bold = true;
            ws.Cells[row, 10].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 10].Style.Font.Size = 14;

            // 🔥 AUTO WIDTH
            ws.Cells.AutoFitColumns();

            // 🔥 BORDER
            using (var range = ws.Cells[5, 1, row, 11])
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
