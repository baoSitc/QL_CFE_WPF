using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using QL_CFE_WPF.Data;
using QL_CFE_WPF.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;


namespace QL_CFE_WPF.Services
{
     public class pdfBillService
    {
       
            public void ExportPdf(HoaDon hoaDon)
            {
                // =============================
                // TẠO PDF
                // =============================
                PdfDocument document =
                  new PdfDocument();

                document.Info.Title = "Phở Gia Lai";
                PdfPage page = document.AddPage();
                // bill K80
                page.Width = XUnit.FromMillimeter(80);

                page.Height = XUnit.FromMillimeter(250);

                XGraphics gfx = XGraphics.FromPdfPage(page);
                // =============================
                // FONT
                // =============================
                XFont fontTitle = new XFont("Verdana", 8, XFontStyle.Regular);
                XFont fontHoaDon = new XFont("Verdana", 10, XFontStyle.Bold);

                XFont fontNormal = new XFont("Verdana", 8, XFontStyle.Regular);

                XFont fontBold = new XFont("Verdana", 10, XFontStyle.Bold);

                XFont fontTenHang = new XFont("Verdana", 8, XFontStyle.Regular);
                int y = 20;
                // =============================
                // SHOP
                // =============================
                gfx.DrawString("Địa chỉ: 43N-Hoàng Quốc Việt-Phường Phú Mỹ-TP.HCM", fontTitle, XBrushes.Black, new XRect(0, y, page.Width, 20),
                    XStringFormats.TopLeft);          
            y += 15;
            gfx.DrawString("Điện thoại: 0283.7853661", fontTitle, XBrushes.Black, new XRect(0, y, page.Width, 20),
                   XStringFormats.TopLeft);
            y += 25;

                gfx.DrawString("HÓA ĐƠN TẠM TÍNH", fontHoaDon, XBrushes.Black, new XRect(0, y, page.Width, 20),
                   XStringFormats.TopCenter);
                    y += 15;
            gfx.DrawString("Lần in: " + hoaDon.LanIn, fontHoaDon, XBrushes.Black, new XRect(0, y, page.Width, 10),
                   XStringFormats.TopCenter);
            y += 15;
            gfx.DrawString("Ngày: " + DateTime.Now.ToString("dd/MM/yyyy"), fontNormal, XBrushes.Black, new XRect(1, y, page.Width, 20),
                XStringFormats.TopLeft);
            
                gfx.DrawString("Số HĐ: " + hoaDon.SoHoaDon?.Substring(9), fontNormal, XBrushes.Black, page.Width - 1, y,
                   XStringFormats.TopRight);

                y += 15;
                gfx.DrawString("Giờ vào: " + hoaDon.GioVao.ToString("HH:mm:ss"), fontNormal, XBrushes.Black, new XRect(1, y, page.Width, 20),
                    XStringFormats.TopLeft);
            using (var db = new AppDbContext())
            {
                var ban = db.Bans.Find(hoaDon.MaBan);
                gfx.DrawString("Bàn: " + ban?.TenBan, fontNormal, XBrushes.Black, page.Width -1 , y,
                   XStringFormats.TopRight);
            }

           
            y += 15;
            gfx.DrawString("Giờ ra: " + hoaDon.GioRa?.ToString("HH:mm:ss"), fontNormal, XBrushes.Black, new XRect(1, y, page.Width, 20),
                XStringFormats.TopLeft);
            gfx.DrawString("NV:" + Session.CurrentUser.TenHienThi, fontNormal, XBrushes.Black, page.Width-1, y,
                XStringFormats.TopRight);


          
               

                y +=35;
                XTextFormatter tf = new XTextFormatter(gfx);

                //XRect rectDiaChi =
                //    new XRect(10, y, 180, 40);

                //tf.DrawString(
                //    "DC: " + hoaDon.DiaChiGiao,
                //    fontNormal,
                //    XBrushes.Black,
                //    rectDiaChi,
                //    XStringFormats.TopLeft);
                //if (hoaDon.DiaChiGiao != null && hoaDon.DiaChiGiao.Length > 23)
                //    y += 40;
                //else y += 25;
                // =============================
                // HEADER
                // =============================

                gfx.DrawString("Đơn Giá", fontBold, XBrushes.Black, 10, y);

                gfx.DrawString("Số lượng", fontBold, XBrushes.Black, 90, y);

                gfx.DrawString("Thành tiền", fontBold, XBrushes.Black, 150, y);

                y += 10;

                gfx.DrawLine(XPens.Black, 10, y, 220, y);

                y += 10;
                // =============================
                // DANH SÁCH HÀNG
                // =============================
                XPen dottedPen = new XPen(XColors.Brown, 1);

                dottedPen.DashStyle = XDashStyle.Dot;
            decimal tongTien = 0;decimal tienGiam = 0;decimal tienVAT = 0;

            foreach (var item in hoaDon.ChiTietHoaDons)
                {
                    gfx.DrawString(item.SanPham.TenSP, fontNormal, XBrushes.Black, 10, y);
                    y += 10;

                    gfx.DrawString(item.Gia.ToString("N0"), fontNormal, XBrushes.Black, 10, y);

                    gfx.DrawString(item.SoLuong.ToString("N2"), fontNormal, XBrushes.Black, 120, y);

                    //gfx.DrawString(    item.ThanhTien.ToString("N0"),  fontNormal,   XBrushes.Black,   160,  y );
                    gfx.DrawString((item.SoLuong * item.Gia).ToString("N0"), fontNormal, XBrushes.Black,
                                    new XRect(150, y - 10, 60, 20),
                                    XStringFormats.TopRight);
                    y += 5;
                    gfx.DrawLine(dottedPen, 10, y, 220, y);
                    y += 10;
                tongTien += item.SoLuong * item.Gia;
            }
            tienGiam = Math.Round((tongTien * hoaDon.GiamGia / 100), 0);
            tienVAT = Math.Round((tongTien - tienGiam) * hoaDon.VAT/100, 0);
               // y += 10;

               // gfx.DrawLine(XPens.Black, 10, y, 220, y);

                y += 10;
                // =============================
                // TỔNG TIỀN
                // =============================

                gfx.DrawString("TỔNG CỘNG:", fontBold, XBrushes.Black, new XRect(50, y, page.Width, 20),
                XStringFormats.TopLeft);
            gfx.DrawString(tongTien.ToString("N0") + " đ", fontBold, XBrushes.Black, page.Width - 1, y,
                XStringFormats.TopRight);

            y += 15;
            if (hoaDon.GiamGia > 0)
            {
                gfx.DrawString("GIẢM GIÁ:"+ hoaDon.GiamGia.ToString("N0") + "%", fontNormal, XBrushes.Black, new XRect(50, y, page.Width, 20),
                XStringFormats.TopLeft);
                gfx.DrawString(tienGiam.ToString("N0") + " đ", fontNormal, XBrushes.Black, page.Width - 1, y,
                XStringFormats.TopRight);
                
            }
            y += 15;
            if (hoaDon.VAT > 0)
            {
                gfx.DrawString("VAT:"+ hoaDon.VAT.ToString("N0") + "%", fontNormal, XBrushes.Black, new XRect(50, y, page.Width, 20),
                XStringFormats.TopLeft);
                gfx.DrawString(tienVAT.ToString("N0") + " đ", fontNormal, XBrushes.Black, page.Width - 1, y,
                XStringFormats.TopRight);
                
            }
            y += 15;
            gfx.DrawString("THÀNH TIỀN:", fontBold, XBrushes.Black, new XRect(50, y, page.Width, 20),
                XStringFormats.TopLeft);
            // decimal thanhTien = tongTien - (tongTien * hoaDon.GiamGia / 100) + (tongTien * hoaDon.VAT / 100);
            gfx.DrawString(hoaDon.ThanhTien.ToString("N0") + " đ", fontBold, XBrushes.Black, page.Width - 1, y,
                XStringFormats.TopRight);


            y += 40;

                // =============================
                // FOOTER
                // =============================

                gfx.DrawString("Cảm ơn quý khách!", fontNormal, XBrushes.Black, new XRect(0, y, page.Width, 20),
                    XStringFormats.TopCenter);

                // =============================
                // SAVE FILE
                // =============================

                string folder =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop),
                        "Bills");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName =
                    $"HD_{hoaDon.MaHD}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                string fullPath =
                    Path.Combine(folder, fileName);

                document.Save(fullPath);

                // mở pdf
                Process.Start(new ProcessStartInfo()
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });

            }
        }
  
}
