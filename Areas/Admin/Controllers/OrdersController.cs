using System;
using System.Data.Entity;
using System.Text;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Quản lý đơn hàng: danh sách (tìm kiếm, sắp xếp, phân trang), chi tiết, cập nhật trạng thái
    [AdminAuthorize]
    public class OrdersController : BaseController
    {
        // GET: Admin/Orders
        // Tìm theo Tên khách hàng, Trạng thái thanh toán, Ngày đặt hàng; 15 đơn/trang
        public ActionResult Index(OrderSearchVM model, int? page)
        {
            var orders = Filter(model);

            switch (model.SortOrder)
            {
                case "date_asc": orders = orders.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderId); break;
                case "name_asc": orders = orders.OrderBy(o => o.User.FullName).ThenByDescending(o => o.OrderDate); break;
                case "name_desc": orders = orders.OrderByDescending(o => o.User.FullName).ThenByDescending(o => o.OrderDate); break;
                case "total_asc": orders = orders.OrderBy(o => o.TotalAmount); break;
                case "total_desc": orders = orders.OrderByDescending(o => o.TotalAmount); break;
                default: orders = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId); break;
            }

            ViewBag.PaymentStatusList = new SelectList(PayStatuses.All.Select(s => new { Value = s, Text = PayStatuses.Text(s) }), "Value", "Text", model.PaymentStatus);
            model.Orders = orders.ToPagedList(BookQueries.Page(page), model.PageSize);
            return View(model);
        }

        private IQueryable<Order> Filter(OrderSearchVM model)
        {
            var orders = db.Orders.Include(o => o.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.CustomerName))
            {
                var name = model.CustomerName.Trim();
                orders = orders.Where(o => o.User.FullName.Contains(name) || o.ReceiverName.Contains(name));
            }
            if (!string.IsNullOrWhiteSpace(model.PaymentStatus))
            {
                var status = model.PaymentStatus;
                orders = orders.Where(o => o.PaymentStatus == status);
            }
            if (model.OrderDate.HasValue)
            {
                var date = model.OrderDate.Value.Date;
                var next = date.AddDays(1);
                orders = orders.Where(o => o.OrderDate >= date && o.OrderDate < next);
            }
            return orders;
        }

        // GET: Admin/Orders/Export  (xuất CSV theo đúng bộ lọc đang chọn, mở được bằng Excel)
        public ActionResult Export(OrderSearchVM model)
        {
            var rows = Filter(model).OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId).ToList();
            Func<string, string> q = v => "\"" + (v ?? "").Replace("\"", "\"\"") + "\"";
            var sb = new StringBuilder();
            sb.AppendLine("Mã đơn,Ngày đặt,Khách hàng,Người nhận,SĐT,Tổng giá trị,Phí ship,Giảm giá,Thanh toán,Trạng thái thanh toán,Trạng thái đơn,Giao hàng");
            foreach (var o in rows)
            {
                sb.AppendLine(string.Join(",", o.OrderId, o.OrderDate.ToString("dd/MM/yyyy HH:mm"), q(o.User.FullName), q(o.ReceiverName), q(o.ReceiverPhone),
                    o.TotalAmount.ToString("0"), o.ShippingFee.ToString("0"), o.DiscountAmount.ToString("0"),
                    q(o.PaymentMethodText), q(o.PaymentStatusText), q(o.StatusText), q(o.DeliveryText)));
            }
            var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", "don-hang-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv");
        }

        // GET: Admin/Orders/DetailedOrder/5
        public ActionResult DetailedOrder(int? id)
        {
            var order = FindOrder(id);
            if (order == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }
            return View(order);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            var order = FindOrder(id);
            if (order == null)
            {
                return id == null ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest) : HttpNotFound();
            }
            ViewBag.NextStatuses = OrderWorkflow.NextStatuses(order);
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        // Trạng thái thanh toán không chọn tay: tự cập nhật theo OrderWorkflow. Hủy đơn: trigger trong CSDL hoàn kho và trả lượt coupon.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus, bool paymentReceived = false)
        {
            var order = FindOrder(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var error = OrderWorkflow.Apply(order, orderStatus, paymentReceived);
            if (error == null)
            {
                error = DbErrors.Save(db);
            }
            if (error != null)
            {
                ModelState.AddModelError("", error);
                db.Entry(order).Reload();
                ViewBag.NextStatuses = OrderWorkflow.NextStatuses(order);
                return View(order);
            }

            var note = OrderWorkflow.RefundNote(order);
            TempData["Success"] = "Đơn hàng #" + order.OrderId + ": " + order.StatusText + " · " + order.PaymentStatusText + (note != null ? ". " + note : "");
            return RedirectToAction("DetailedOrder", new { id = order.OrderId });
        }

        private Order FindOrder(int? id)
        {
            if (id == null)
            {
                return null;
            }
            return db.Orders.Include(o => o.User).Include(o => o.Coupon)
                            .Include(o => o.OrderItems.Select(oi => oi.Book))
                            .FirstOrDefault(o => o.OrderId == id);
        }
    }
}
