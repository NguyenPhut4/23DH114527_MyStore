using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Cart  (gio hang nang cao: nhom theo danh muc + san pham tuong tu)
        public ActionResult Index(int? page)
        {
            var cart = GetCartService().GetCart();

            // Cap nhat lai ton kho / gia moi nhat cho tung mat hang
            var ids = cart.Items.Select(i => i.ProductID).ToList();
            var products = db.Products.Include(p => p.Category).Where(p => ids.Contains(p.ProductID)).ToList();
            foreach (var item in cart.Items.ToList())
            {
                var p = products.FirstOrDefault(x => x.ProductID == item.ProductID);
                if (p == null)
                {
                    cart.RemoveItem(item.ProductID);
                    continue;
                }
                item.MaxQuantity = p.Quantity;
                item.UnitPrice = p.ProductPrice;
                if (p.Quantity <= 0)
                {
                    item.Selected = false;
                }
                else if (item.Quantity > p.Quantity)
                {
                    item.Quantity = p.Quantity;
                }
            }

            var categoryIds = products.Select(p => p.CategoryID).Distinct().ToList();
            cart.SimilarProducts = db.Products
                .Where(p => categoryIds.Contains(p.CategoryID) && !ids.Contains(p.ProductID) && p.Quantity > 0)
                .OrderByDescending(p => p.SoldQuantity)
                .ToPagedList(page ?? 1, 4);

            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity = 1, bool buyNow = false, string returnUrl = null)
        {
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                return HttpNotFound();
            }
            if (product.Quantity <= 0)
            {
                TempData["Error"] = "Sản phẩm \"" + product.ProductName + "\" đã hết hàng.";
                return RedirectBack(returnUrl);
            }

            var cart = GetCartService().GetCart();
            var before = cart.Items.Where(i => i.ProductID == productId).Select(i => i.Quantity).FirstOrDefault();
            var after = cart.AddItem(product, quantity);

            if (before + quantity > after)
            {
                TempData["Error"] = "Chỉ còn " + product.Quantity + " sản phẩm \"" + product.ProductName + "\" trong kho. Giỏ hàng đã được cập nhật tối đa.";
            }
            else
            {
                TempData["Success"] = "Đã thêm " + quantity + " \"" + product.ProductName + "\" vào giỏ hàng.";
            }

            // Sau khi them vao gio -> chuyen toi trang gio hang
            return buyNow || string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index") : RedirectBack(returnUrl);
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int productId)
        {
            GetCartService().GetCart().RemoveItem(productId);
            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int quantity, int? setQuantity)
        {
            // Nut +/- gui setQuantity; o nhap so gui quantity
            if (setQuantity.HasValue)
            {
                quantity = setQuantity.Value;
            }
            var cart = GetCartService().GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                var stock = db.Products.Where(p => p.ProductID == productId).Select(p => p.Quantity).FirstOrDefault();
                item.MaxQuantity = stock;
                if (quantity > stock)
                {
                    TempData["Error"] = "Chỉ còn " + stock + " sản phẩm \"" + item.ProductName + "\" trong kho.";
                }
                cart.UpdateQuantity(productId, quantity < 1 ? 1 : quantity);
            }
            return RedirectToAction("Index");
        }

        // POST: Cart/ToggleSelect  (tickbox chon mua)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleSelect(int? productId, bool selected = false)
        {
            var cart = GetCartService().GetCart();
            if (productId.HasValue)
            {
                cart.SetSelected(productId.Value, selected);
            }
            else
            {
                cart.SelectAll(selected);
            }
            return RedirectToAction("Index");
        }

        // POST: Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        private ActionResult RedirectBack(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
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
