using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        private const int RecentPerStatus = 2;   // mỗi trạng thái chỉ hiện 2 đơn gần nhất
        private const int MaxPerStatus = 50;     // "Xem thêm": tối đa 50 đơn gần nhất

        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            var lines = LoadCartLines();
            if (lines.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "Cart");
            }
            var user = CurrentUser;
            var vm = new CheckoutVM
            {
                ReceiverName = user.FullName,
                ReceiverPhone = user.PhoneNumber,
                ShippingAddress = user.Address,   // mặc định lấy thông tin đã lưu trong tài khoản
                DeliveryMethod = DeliveryOptions.Standard,
                PaymentMethod = PayMethods.Cod
            };
            Fill(vm, lines);
            return View(vm);
        }

        // POST: Order/Checkout  (submit = "apply": áp mã giảm giá; "order": đặt hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model, string submit)
        {
            var lines = LoadCartLines();
            if (lines.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            if (!DeliveryOptions.All.Contains(model.DeliveryMethod ?? ""))
            {
                ModelState.AddModelError("DeliveryMethod", "Hình thức giao hàng không hợp lệ");
            }
            if (!PayMethods.All.Contains(model.PaymentMethod ?? ""))
            {
                ModelState.AddModelError("PaymentMethod", "Hình thức thanh toán không hợp lệ");
            }

            var subTotal = lines.Sum(l => l.LineTotal);
            decimal discount;
            var couponError = CouponRules.Evaluate(db, model.CouponCode, subTotal, out discount);

            if (submit == "apply")
            {
                ModelState.Clear();
                Fill(model, lines);
                model.CouponMessage = string.IsNullOrWhiteSpace(model.CouponCode) ? "Bạn chưa nhập mã giảm giá."
                    : couponError ?? ("Áp dụng thành công, giảm " + discount.ToString("N0") + " đ.");
                model.Discount = couponError == null ? discount : 0;
                return View(model);
            }

            if (couponError != null)
            {
                ModelState.AddModelError("CouponCode", couponError);
            }
            foreach (var l in lines.Where(x => !x.Available))
            {
                ModelState.AddModelError("", "Sách \"" + l.Book.Title + "\" chỉ còn " + Math.Max(0, l.Book.StockQuantity) + " cuốn trong kho. Vui lòng cập nhật giỏ hàng.");
            }
            if (!ModelState.IsValid)
            {
                Fill(model, lines);
                return View(model);
            }

            // Đặt hàng bằng stored procedure (một transaction: tạo đơn, chi tiết, trừ kho bằng trigger, tăng lượt coupon, xóa giỏ)
            var fee = DeliveryOptions.Fee(subTotal, model.DeliveryMethod);
            var code = string.IsNullOrWhiteSpace(model.CouponCode) ? null : model.CouponCode.Trim().ToUpperInvariant();
            try
            {
                var orderId = db.Database.SqlQuery<int>(
                    "EXEC dbo.sp_PlaceOrder @UserId, @ReceiverName, @ReceiverPhone, @ShippingAddress, @PaymentMethod, @ShippingFee, @CouponCode, @DeliveryMethod",
                    new SqlParameter("@UserId", CurrentUser.UserId),
                    new SqlParameter("@ReceiverName", model.ReceiverName.Trim()),
                    new SqlParameter("@ReceiverPhone", model.ReceiverPhone.Trim()),
                    new SqlParameter("@ShippingAddress", model.ShippingAddress.Trim()),
                    new SqlParameter("@PaymentMethod", model.PaymentMethod),
                    new SqlParameter("@ShippingFee", fee),
                    new SqlParameter("@CouponCode", (object)code ?? DBNull.Value),
                    new SqlParameter("@DeliveryMethod", model.DeliveryMethod)).First();
                return RedirectToAction("OrderSuccess", new { id = orderId });
            }
            catch (Exception ex)
            {
                var sql = ex.GetBaseException() as SqlException;
                ModelState.AddModelError("", sql != null && sql.Number == 547
                    ? "Một số sách không đủ số lượng trong kho. Vui lòng cập nhật giỏ hàng."
                    : "Đặt hàng không thành công: " + (sql != null ? sql.Message : "vui lòng thử lại."));
                Fill(model, lines);
                return View(model);
            }
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int? id)
        {
            var order = FindMyOrder(id);
            return order == null ? (ActionResult)HttpNotFound() : View(order);
        }

        // GET: Order/OrderDetail/5  (hóa đơn, dùng chung partial với Admin)
        public ActionResult OrderDetail(int? id)
        {
            var order = FindMyOrder(id);
            return order == null ? (ActionResult)HttpNotFound() : View(order);
        }

        // GET: Order/MyOrder?status=&expand=&searchTerm=
        public ActionResult MyOrder(string status, string expand, string searchTerm)
        {
            var uid = CurrentUser.UserId;
            var vm = new MyOrderVM
            {
                Status = OrderStatuses.All.Contains(status ?? "") ? status : null,
                Expand = expand,
                SearchTerm = searchTerm
            };

            var myOrders = db.Orders.Where(o => o.UserId == uid);
            vm.HasAnyOrder = myOrders.Any();

            // Tìm theo Mã đơn hàng hoặc Tên sản phẩm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().TrimStart('#');
                int orderId;
                myOrders = int.TryParse(term, out orderId)
                    ? myOrders.Where(o => o.OrderId == orderId || o.OrderItems.Any(oi => oi.Book.Title.Contains(term)))
                    : myOrders.Where(o => o.OrderItems.Any(oi => oi.Book.Title.Contains(term)));
            }

            var counts = myOrders.GroupBy(o => o.Status).Select(g => new { g.Key, Count = g.Count() }).ToList();
            foreach (var s in OrderStatuses.All)
            {
                vm.StatusCounts[s] = counts.Where(c => c.Key == s).Select(c => c.Count).FirstOrDefault();
            }

            var statuses = vm.Status == null ? OrderStatuses.All : new[] { vm.Status };
            foreach (var s in statuses)
            {
                var total = vm.StatusCounts[s];
                if (total == 0)
                {
                    continue;
                }
                var expanded = expand == s;
                var orders = myOrders.Where(o => o.Status == s)
                                     .Include(o => o.OrderItems.Select(oi => oi.Book))
                                     .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderId)
                                     .Take(expanded ? MaxPerStatus : RecentPerStatus)
                                     .ToList();
                vm.Groups.Add(new OrderGroup { Status = s, TotalCount = total, Expanded = expanded, Orders = orders });
            }
            return View(vm);
        }

        // POST: Order/BuyAgain  (thêm sách vào giỏ với số lượng = 1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int? orderId, int? bookId)
        {
            var uid = CurrentUser.UserId;
            List<int> bookIds;
            if (bookId.HasValue)
            {
                bookIds = new List<int> { bookId.Value };
            }
            else
            {
                var order = FindMyOrder(orderId);
                if (order == null)
                {
                    return HttpNotFound();
                }
                bookIds = order.OrderItems.Select(oi => oi.BookId).ToList();
            }

            var added = 0;
            var unavailable = new List<string>();
            foreach (var book in db.Books.Where(b => bookIds.Contains(b.BookId)).ToList())
            {
                if (!book.IsActive || book.StockQuantity <= 0)
                {
                    unavailable.Add(book.Title);
                    continue;
                }
                var item = db.CartItems.FirstOrDefault(c => c.UserId == uid && c.BookId == book.BookId);
                if (item == null)
                {
                    db.CartItems.Add(new CartItem { UserId = uid, BookId = book.BookId, Quantity = 1, AddedAt = DateTime.Now });
                }
                else
                {
                    item.Quantity = 1;   // Mua lại: số lượng = 1
                }
                added++;
            }
            var error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("MyOrder");
            }
            if (unavailable.Any())
            {
                TempData["Error"] = "Hết hàng hoặc ngừng kinh doanh: " + string.Join(", ", unavailable);
            }
            if (added > 0)
            {
                TempData["Success"] = "Đã thêm " + added + " sách vào giỏ hàng với số lượng = 1.";
            }
            return RedirectToAction("Index", "Cart");
        }

        // POST: Order/Cancel  (khách chỉ hủy được đơn đang chờ: trigger hoàn kho và trả lượt coupon)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var order = FindMyOrder(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            if (order.Status != OrderStatuses.Pending)
            {
                TempData["Error"] = "Chỉ hủy được đơn hàng đang ở trạng thái \"" + OrderStatuses.Text(OrderStatuses.Pending) + "\".";
                return RedirectToAction("OrderDetail", new { id });
            }
            order.Status = OrderStatuses.Cancelled;
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã hủy đơn hàng #" + id + ".";
            return RedirectToAction("MyOrder");
        }

        private List<CartLine> LoadCartLines()
        {
            var uid = CurrentUser.UserId;
            return db.CartItems.Include(c => c.Book.Category).Where(c => c.UserId == uid).OrderBy(c => c.AddedAt).ToList()
                     .Select(c => new CartLine { Book = c.Book, Quantity = c.Quantity }).ToList();
        }

        private static void Fill(CheckoutVM vm, List<CartLine> lines)
        {
            vm.Lines = lines;
            vm.SubTotal = lines.Sum(l => l.LineTotal);
            vm.ShippingFee = DeliveryOptions.Fee(vm.SubTotal, vm.DeliveryMethod ?? DeliveryOptions.Standard);
        }

        private Order FindMyOrder(int? id)
        {
            if (id == null)
            {
                return null;
            }
            var uid = CurrentUser.UserId;
            return db.Orders.Include(o => o.User).Include(o => o.Coupon)
                            .Include(o => o.OrderItems.Select(oi => oi.Book))
                            .FirstOrDefault(o => o.OrderId == id && o.UserId == uid);
        }
    }
}
