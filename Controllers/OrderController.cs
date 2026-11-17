using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const int RecentPerStatus = 2;   // moi trang thai chi hien 2 don gan nhat
        private const int MaxPerStatus = 50;     // "Xem them": toi da 50 don gan nhat

        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            var cart = new CartService(Session).GetCart();
            if (!cart.SelectedItems.Any())
            {
                TempData["Error"] = "Giỏ hàng chưa có sản phẩm nào được chọn mua.";
                return RedirectToAction("Index", "Cart");
            }

            var customer = CurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout") });
            }

            var vm = new CheckoutVM
            {
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone,
                ShippingAddress = customer.CustomerAddress,   // mac dinh lay dia chi da luu
                DeliveryMethod = DeliveryMethods.Fast,
                PaymentMethod = PaymentMethods.Cash
            };
            FillCart(vm, cart);
            return View(vm);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = new CartService(Session).GetCart();
            var selected = cart.SelectedItems.ToList();
            if (!selected.Any())
            {
                return RedirectToAction("Index", "Home");
            }
            var customer = CurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!DeliveryMethods.All.Contains(model.DeliveryMethod))
            {
                ModelState.AddModelError("DeliveryMethod", "Hình thức giao hàng không hợp lệ");
            }
            if (!PaymentMethods.All.Contains(model.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Hình thức thanh toán không hợp lệ");
            }

            // Kiem tra ton kho tai thoi diem dat hang
            var ids = selected.Select(i => i.ProductID).ToList();
            var products = db.Products.Where(p => ids.Contains(p.ProductID)).ToList();
            foreach (var item in selected)
            {
                var p = products.FirstOrDefault(x => x.ProductID == item.ProductID);
                if (p == null || p.Quantity < item.Quantity)
                {
                    ModelState.AddModelError("", "Sản phẩm \"" + item.ProductName + "\" chỉ còn " + (p == null ? 0 : p.Quantity) + " trong kho.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.CustomerName = customer.CustomerName;
                model.CustomerPhone = customer.CustomerPhone;
                FillCart(model, cart);
                return View(model);
            }

            // Chuyen khoan -> cho thanh toan; tien mat (COD) -> dang xu ly
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Today,
                ShippingAddress = model.ShippingAddress.Trim(),
                DeliveryMethod = model.DeliveryMethod,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = PaymentStatuses.Unpaid,
                OrderStatus = model.PaymentMethod == PaymentMethods.BankTransfer ? OrderStatuses.Pending : OrderStatuses.Processing
            };

            foreach (var item in selected)
            {
                var p = products.First(x => x.ProductID == item.ProductID);
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductID = p.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = p.ProductPrice
                });
                // Tinh lai so luong ton va so luong da ban
                p.Quantity -= item.Quantity;
                p.SoldQuantity += item.Quantity;
            }
            order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);

            db.Orders.Add(order);
            // CK_Product_Quantity chan ton kho am neu 2 nguoi cung mua mon cuoi cung
            var error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = "Đặt hàng không thành công: " + error + " Vui lòng kiểm tra lại giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            // Chi bo cac san pham da mua ra khoi gio
            cart.RemoveSelected();

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int? id)
        {
            var order = FindMyOrder(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Order/OrderDetail/5  (hoa don - dung chung partial voi Admin)
        public ActionResult OrderDetail(int? id)
        {
            var order = FindMyOrder(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Order/MyOrder?status=&expand=&searchTerm=
        public ActionResult MyOrder(string status, string expand, string searchTerm)
        {
            var customer = CurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = new MyOrderVM
            {
                Status = OrderStatuses.All.Contains(status) ? status : null,
                Expand = expand,
                SearchTerm = searchTerm
            };

            var myOrders = db.Orders.Where(o => o.CustomerID == customer.CustomerID);
            vm.HasAnyOrder = myOrders.Any();

            // Tim theo Ma don hang hoac Ten san pham
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().TrimStart('#');
                int orderId;
                if (int.TryParse(term, out orderId))
                {
                    myOrders = myOrders.Where(o => o.OrderID == orderId
                                                || o.OrderDetails.Any(od => od.Product.ProductName.Contains(term)));
                }
                else
                {
                    myOrders = myOrders.Where(o => o.OrderDetails.Any(od => od.Product.ProductName.Contains(term)));
                }
            }

            var counts = myOrders.GroupBy(o => o.OrderStatus)
                                 .Select(g => new { g.Key, Count = g.Count() })
                                 .ToList();
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
                var orders = myOrders.Where(o => o.OrderStatus == s)
                                     .Include(o => o.OrderDetails.Select(od => od.Product))
                                     .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID)
                                     .Take(expanded ? MaxPerStatus : RecentPerStatus)
                                     .ToList();
                vm.Groups.Add(new OrderGroup { Status = s, TotalCount = total, Expanded = expanded, Orders = orders });
            }

            return View(vm);
        }

        // POST: Order/BuyAgain  (them san pham vao gio voi so luong = 1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int? orderId, int? productId)
        {
            var customer = CurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<int> productIds;
            if (productId.HasValue)
            {
                productIds = new List<int> { productId.Value };
            }
            else
            {
                var order = FindMyOrder(orderId);
                if (order == null)
                {
                    return HttpNotFound();
                }
                productIds = order.OrderDetails.Select(od => od.ProductID).ToList();
            }

            var cart = new CartService(Session).GetCart();
            var products = db.Products.Include(p => p.Category).Where(p => productIds.Contains(p.ProductID)).ToList();
            var added = 0;
            var outOfStock = new List<string>();
            foreach (var p in products)
            {
                if (p.Quantity <= 0)
                {
                    outOfStock.Add(p.ProductName);
                    continue;
                }
                // De Ktra: MUA LAI -> them vao gio voi so luong = 1
                cart.SetItemQuantityOne(p);
                added++;
            }

            if (outOfStock.Any())
            {
                TempData["Error"] = "Hết hàng: " + string.Join(", ", outOfStock);
            }
            if (added > 0)
            {
                TempData["Success"] = "Đã thêm " + added + " sản phẩm vào giỏ hàng với số lượng = 1.";
            }
            return RedirectToAction("Index", "Cart");
        }

        private Customer CurrentCustomer()
        {
            var username = User.Identity.Name;
            return db.Customers.FirstOrDefault(c => c.Username == username);
        }

        private Order FindMyOrder(int? id)
        {
            if (id == null)
            {
                return null;
            }
            var username = User.Identity.Name;
            return db.Orders.Include(o => o.Customer)
                            .Include(o => o.OrderDetails.Select(od => od.Product))
                            .FirstOrDefault(o => o.OrderID == id && o.Customer.Username == username);
        }

        private static void FillCart(CheckoutVM vm, Cart cart)
        {
            vm.Items = cart.SelectedItems.ToList();
            vm.TotalAmount = cart.SelectedTotal;
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
