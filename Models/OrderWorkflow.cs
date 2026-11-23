using System.Collections.Generic;
using System.Linq;

namespace NguyenTanPhu_23DH114527.Models
{
    /*
     * Quy tắc chuyển trạng thái đơn hàng và trạng thái thanh toán.
     * Trạng thái thanh toán không chọn tay: hệ thống tự cập nhật; CSDL còn có ràng buộc
     * CK_Orders_StatusPayment và trigger trg_Orders_StatusChange chặn các trường hợp vô lý.
     *
     *   Trả trước (BANKING, EWALLET): Pending -> (xác nhận đã nhận tiền) Confirmed -> Shipping -> Completed
     *                                 Pending, Confirmed có thể Cancelled (đã thu tiền thì cần hoàn tiền).
     *   COD:                          Pending -> Confirmed -> Shipping -> Completed (giao xong tự thành Paid)
     *                                 Pending, Confirmed, Shipping có thể Cancelled.
     *   Completed và Cancelled là trạng thái cuối. Hủy đơn: trigger hoàn kho và trả lượt coupon.
     */
    public static class OrderWorkflow
    {
        public static bool IsFinal(Order order)
        {
            return order.Status == OrderStatuses.Completed || order.Status == OrderStatuses.Cancelled;
        }

        public static IList<string> NextStatuses(Order order)
        {
            var prepaid = PayMethods.IsPrepaid(order.PaymentMethod);
            switch (order.Status)
            {
                case OrderStatuses.Pending:
                    return new[] { OrderStatuses.Confirmed, OrderStatuses.Cancelled };
                case OrderStatuses.Confirmed:
                    return new[] { OrderStatuses.Shipping, OrderStatuses.Cancelled };
                case OrderStatuses.Shipping:
                    return prepaid
                        ? new[] { OrderStatuses.Completed }
                        : new[] { OrderStatuses.Completed, OrderStatuses.Cancelled };
                default:
                    return new string[0];
            }
        }

        /// <returns>null nếu thành công, ngược lại là thông báo lỗi.</returns>
        public static string Apply(Order order, string newStatus, bool paymentReceived)
        {
            if (IsFinal(order))
            {
                return "Đơn hàng đã ở trạng thái \"" + OrderStatuses.Text(order.Status) + "\", không thể thay đổi.";
            }
            if (!NextStatuses(order).Contains(newStatus))
            {
                return "Không thể chuyển từ \"" + OrderStatuses.Text(order.Status) + "\" sang \"" + OrderStatuses.Text(newStatus) + "\".";
            }

            if (order.Status == OrderStatuses.Pending && newStatus == OrderStatuses.Confirmed && PayMethods.IsPrepaid(order.PaymentMethod))
            {
                if (!paymentReceived)
                {
                    return "Đơn trả trước phải xác nhận \"Đã nhận tiền\" trước khi chuyển sang \"Đang xử lý\".";
                }
                order.PaymentStatus = PayStatuses.Paid;
            }

            if (newStatus == OrderStatuses.Completed)
            {
                order.PaymentStatus = PayStatuses.Paid;   // COD: giao thành công là đã thu tiền
            }

            order.Status = newStatus;
            return null;
        }

        public static string RefundNote(Order order)
        {
            return order.Status == OrderStatuses.Cancelled && order.PaymentStatus == PayStatuses.Paid
                ? "Đơn đã hủy sau khi khách trả trước: cần hoàn tiền cho khách."
                : null;
        }
    }
}
