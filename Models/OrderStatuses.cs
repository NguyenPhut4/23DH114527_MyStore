using System.Collections.Generic;

namespace NguyenTanPhu_23DH114527.Models
{
    // Cac gia tri trang thai dung chung cho Admin va Customer
    public static class OrderStatuses
    {
        public const string Pending = "Chờ thanh toán";
        public const string Processing = "Đang xử lý";
        public const string Shipping = "Đang vận chuyển";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        public static readonly IReadOnlyList<string> All = new[] { Pending, Processing, Shipping, Delivered, Cancelled };
    }

    public static class PaymentStatuses
    {
        public const string Unpaid = "Chưa thanh toán";
        public const string Paid = "Đã thanh toán";

        public static readonly IReadOnlyList<string> All = new[] { Unpaid, Paid };
    }

    public static class DeliveryMethods
    {
        public const string Fast = "Giao hàng nhanh";
        public const string Economy = "Giao hàng tiết kiệm";

        public static readonly IReadOnlyList<string> All = new[] { Fast, Economy };
    }

    public static class PaymentMethods
    {
        public const string Cash = "Thanh toán tiền mặt";
        public const string BankTransfer = "Chuyển khoản";

        public static readonly IReadOnlyList<string> All = new[] { Cash, BankTransfer };
    }
}
