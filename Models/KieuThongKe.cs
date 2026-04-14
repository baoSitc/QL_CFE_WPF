using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QL_CFE_WPF.Models
{
    public enum KieuThongKe
    {
        [Description("Hôm nay")]
        HomNay,

        [Description("Hôm qua")]
        HomQua,

        [Description("7 ngày qua")]
        BayNgayQua,

        [Description("Tháng này")]
        ThangNay,

        [Description("Tháng trước")]
        ThangTruoc
    }
}
