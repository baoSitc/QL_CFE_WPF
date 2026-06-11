using QL_CFE_WPF.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace QL_CFE_WPF.Services
{
    public class HoaDonService
    {
        private readonly AppDbContext _db;

        public HoaDonService(AppDbContext db)
        {
            _db = db;
        }

        public string TaoSoHoaDon()
        {
            string ngay = DateTime.Today.ToString("yyyyMMdd");

            string prefix = ngay + "-";

            var soCuoi = _db.HoaDons
                .Where(x => x.SoHoaDon != null &&
                            x.SoHoaDon.StartsWith(prefix))
                .OrderByDescending(x => x.SoHoaDon)
                .Select(x => x.SoHoaDon)
                .FirstOrDefault();

            int stt = 1;

            if (!string.IsNullOrEmpty(soCuoi))
            {
                var arr = soCuoi.Split('-');

                stt = int.Parse(arr[1]) + 1;
            }

            return $"{ngay}-{stt:000}";
        }
    }
}
