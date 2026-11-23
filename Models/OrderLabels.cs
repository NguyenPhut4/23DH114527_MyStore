using System.Collections.Generic;
using System.Linq;

namespace NguyenTanPhu_23DH114527.Models
{
    // Trạng thái đơn hàng lưu trong CSDL (Orders.Status) và cách hiển thị tiếng Việt
    public static class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Shipping = "Shipping";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = { Pending, Confirmed, Shipping, Completed, Cancelled };

        public static string Text(string status)
        {
            switch (status)
            {
                case Pending: return "Chờ thanh toán";
                case Confirmed: return "Đang xử lý";
                case Shipping: return "Đang vận chuyển";
                case Completed: return "Đã giao";
                case Cancelled: return "Đã hủy";
                default: return status;
            }
        }

        public static int Index(string status)
        {
            return System.Array.IndexOf(All, status);
        }
    }

    public static class PayMethods
    {
        public const string Cod = "COD";
        public const string Banking = "BANKING";
        public const string EWallet = "EWALLET";

        public static readonly string[] All = { Cod, Banking, EWallet };

        public static string Text(string method)
        {
            switch (method)
            {
                case Cod: return "Thanh toán khi nhận hàng (COD)";
                case Banking: return "Chuyển khoản ngân hàng";
                case EWallet: return "Ví điện tử";
                default: return method;
            }
        }

        // Chuyển khoản và ví điện tử là trả trước, COD là trả khi nhận hàng
        public static bool IsPrepaid(string method)
        {
            return method != Cod;
        }
    }

    public static class PayStatuses
    {
        public const string Unpaid = "Unpaid";
        public const string Paid = "Paid";

        public static readonly string[] All = { Unpaid, Paid };

        public static string Text(string status)
        {
            return status == Paid ? "Đã thanh toán" : "Chưa thanh toán";
        }
    }

    public static class DeliveryOptions
    {
        public const string Standard = "STANDARD";
        public const string Express = "EXPRESS";

        public static readonly string[] All = { Standard, Express };

        public const decimal StandardFee = 30000m;
        public const decimal ExpressFee = 45000m;
        public const decimal FreeShipFrom = 300000m;   // giao tiết kiệm miễn phí cho đơn từ mức này

        public static string Text(string method)
        {
            return method == Express ? "Giao hàng nhanh (1-2 ngày)" : "Giao hàng tiết kiệm (3-5 ngày)";
        }

        public static decimal Fee(decimal subTotal, string method)
        {
            if (method == Express)
            {
                return ExpressFee;
            }
            return subTotal >= FreeShipFrom ? 0m : StandardFee;
        }
    }

    public static class UserRoles
    {
        public const string Customer = "Customer";
        public const string Staff = "Staff";
        public const string Admin = "Admin";

        public static readonly string[] All = { Customer, Staff, Admin };

        public static string Text(string role)
        {
            switch (role)
            {
                case Admin: return "Quản trị viên";
                case Staff: return "Nhân viên";
                default: return "Khách hàng";
            }
        }
    }
}
