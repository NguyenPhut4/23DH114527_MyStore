using System.Collections.Generic;
using System.Linq;

namespace NguyenTanPhu_23DH114527.Models
{
    /*
     * Quy tac chuyen trang thai don hang va trang thai thanh toan (dung chung Admin + DB).
     *
     *  CHUYEN KHOAN (tra truoc):
     *    Cho thanh toan --(xac nhan da nhan tien)--> Dang xu ly --> Dang van chuyen --> Da giao
     *          \--> Da huy                  \--> Da huy (da thu tien -> can hoan tien)
     *    - Phai "Da thanh toan" truoc khi xu ly / van chuyen / giao.
     *
     *  TIEN MAT - COD (tra khi nhan hang):
     *    Dang xu ly --> Dang van chuyen --(giao thanh cong, thu tien)--> Da giao
     *          \--> Da huy          \--> Da huy (khach khong nhan hang)
     *    - Khong co buoc "Cho thanh toan"; chi "Da thanh toan" khi "Da giao".
     *
     *  "Da giao" va "Da huy" la trang thai cuoi, khong doi duoc nua.
     *  Trang thai thanh toan khong chon tay: he thong tu cap nhat theo quy tac tren.
     */
    public static class OrderWorkflow
    {
        public static bool IsBankTransfer(Order order)
        {
            return order.PaymentMethod == PaymentMethods.BankTransfer;
        }

        public static bool IsFinal(Order order)
        {
            return order.OrderStatus == OrderStatuses.Delivered || order.OrderStatus == OrderStatuses.Cancelled;
        }

        // Cac trang thai duoc phep chuyen tiep tu trang thai hien tai
        public static IList<string> NextStatuses(Order order)
        {
            var transfer = IsBankTransfer(order);
            switch (order.OrderStatus)
            {
                case OrderStatuses.Pending:
                    return new[] { OrderStatuses.Processing, OrderStatuses.Cancelled };
                case OrderStatuses.Processing:
                    return new[] { OrderStatuses.Shipping, OrderStatuses.Cancelled };
                case OrderStatuses.Shipping:
                    return transfer
                        ? new[] { OrderStatuses.Delivered }
                        : new[] { OrderStatuses.Delivered, OrderStatuses.Cancelled };
                default:
                    return new string[0];
            }
        }

        /// <summary>Ap dung chuyen trang thai. Tra ve null neu thanh cong, nguoc lai la thong bao loi.</summary>
        /// <param name="paymentReceived">Admin xac nhan da nhan tien chuyen khoan (chi dung khi don dang Cho thanh toan).</param>
        public static string Apply(Order order, string newStatus, bool paymentReceived)
        {
            if (IsFinal(order))
            {
                return "Đơn hàng đã ở trạng thái \"" + order.OrderStatus + "\", không thể thay đổi (không hợp lệ).";
            }
            if (!NextStatuses(order).Contains(newStatus))
            {
                return "Không thể chuyển từ \"" + order.OrderStatus + "\" sang \"" + newStatus + "\" (không hợp lệ).";
            }

            if (order.OrderStatus == OrderStatuses.Pending && newStatus == OrderStatuses.Processing)
            {
                if (!paymentReceived)
                {
                    return "Đơn chuyển khoản phải xác nhận \"Đã nhận tiền\" trước khi chuyển sang \"Đang xử lý\".";
                }
                order.PaymentStatus = PaymentStatuses.Paid;
            }

            // COD: giao thanh cong = da thu tien
            if (newStatus == OrderStatuses.Delivered)
            {
                order.PaymentStatus = PaymentStatuses.Paid;
            }

            order.OrderStatus = newStatus;
            return null;
        }

        // Ghi chu hien tren trang cap nhat
        public static string RefundNote(Order order)
        {
            return order.OrderStatus == OrderStatuses.Cancelled && order.PaymentStatus == PaymentStatuses.Paid
                ? "Đơn đã hủy sau khi khách chuyển khoản: cần hoàn tiền cho khách."
                : null;
        }
    }
}
