using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Quan ly don hang (de Ktra BE): xem, tim kiem, sap xep, phan trang, chi tiet, cap nhat trang thai
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders
        // Tim theo Ten khach hang, Trang thai thanh toan, Ngay dat hang; 15 don/trang
        public ActionResult Index(OrderSearchVM model, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.CustomerName))
            {
                var name = model.CustomerName.Trim();
                orders = orders.Where(o => o.Customer.CustomerName.Contains(name));
            }
            if (!string.IsNullOrWhiteSpace(model.PaymentStatus))
            {
                var status = model.PaymentStatus;
                orders = orders.Where(o => o.PaymentStatus == status);
            }
            if (model.OrderDate.HasValue)
            {
                var date = model.OrderDate.Value.Date;
                orders = orders.Where(o => o.OrderDate == date);
            }

            switch (model.SortOrder)
            {
                case "date_asc": orders = orders.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderID); break;
                case "name_asc": orders = orders.OrderBy(o => o.Customer.CustomerName).ThenByDescending(o => o.OrderDate); break;
                case "name_desc": orders = orders.OrderByDescending(o => o.Customer.CustomerName).ThenByDescending(o => o.OrderDate); break;
                case "total_asc": orders = orders.OrderBy(o => o.TotalAmount); break;
                case "total_desc": orders = orders.OrderByDescending(o => o.TotalAmount); break;
                default: orders = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID); break;
            }

            ViewBag.PaymentStatusList = new SelectList(PaymentStatuses.All, model.PaymentStatus);
            model.Orders = orders.ToPagedList(page ?? 1, model.PageSize);
            return View(model);
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
            LoadStatusLists(order);
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        // Trang thai thanh toan khong chon tay: tu cap nhat theo quy tac trong Models/OrderWorkflow.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus, bool paymentReceived = false)
        {
            var order = FindOrder(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var willCancel = orderStatus == OrderStatuses.Cancelled;
            var error = OrderWorkflow.Apply(order, orderStatus, paymentReceived);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                LoadStatusLists(order);
                return View(order);
            }

            // Huy don -> tra hang ve kho
            if (willCancel)
            {
                foreach (var od in order.OrderDetails)
                {
                    od.Product.Quantity += od.Quantity;
                    od.Product.SoldQuantity = System.Math.Max(0, od.Product.SoldQuantity - od.Quantity);
                }
            }

            error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("UpdateStatus", new { id });
            }

            TempData["Success"] = "Đơn hàng #" + order.OrderID + ": " + order.OrderStatus + " · " + order.PaymentStatus
                                  + (OrderWorkflow.RefundNote(order) != null ? ". " + OrderWorkflow.RefundNote(order) : "");
            return RedirectToAction("DetailedOrder", new { id = order.OrderID });
        }

        private Order FindOrder(int? id)
        {
            if (id == null)
            {
                return null;
            }
            return db.Orders
                     .Include(o => o.Customer)
                     .Include(o => o.OrderDetails.Select(od => od.Product))
                     .FirstOrDefault(o => o.OrderID == id);
        }

        private void LoadStatusLists(Order order)
        {
            // Chi cho chon cac trang thai hop le tiep theo
            ViewBag.NextStatuses = OrderWorkflow.NextStatuses(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
