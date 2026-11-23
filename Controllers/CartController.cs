using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Controllers
{
    // Giỏ hàng lưu trong bảng CartItems (quan hệ nhiều-nhiều Users - Books), nên đăng nhập ở thiết bị khác vẫn thấy giỏ
    public class CartController : BaseController
    {
        private const int MaxPerAdd = 99;

        // GET: Cart
        [Authorize]
        public ActionResult Index(int? page)
        {
            var uid = CurrentUser.UserId;
            var items = db.CartItems.Include(c => c.Book.Category).Include(c => c.Book.BookAuthors.Select(a => a.Author))
                                    .Where(c => c.UserId == uid).OrderBy(c => c.AddedAt).ToList();
            var vm = new CartVM { Lines = items.Select(c => new CartLine { Book = c.Book, Quantity = c.Quantity }).ToList() };

            // Sách cùng danh mục với các sách trong giỏ
            var inCart = items.Select(c => c.BookId).ToList();
            var categoryIds = items.Select(c => c.Book.CategoryId).Distinct().ToList();
            var similar = BookQueries.Available(db).Where(b => categoryIds.Contains(b.CategoryId) && !inCart.Contains(b.BookId))
                                     .WithAuthors().OrderByBestSelling().ToList();
            BookQueries.FillSold(db, similar);
            vm.SimilarBooks = similar.ToPagedList(BookQueries.Page(page), 4);
            return View(vm);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int bookId, int quantity = 1, bool buyNow = false, string returnUrl = null)
        {
            if (CurrentUser == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sách vào giỏ hàng.";
                var back = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("ProductDetail", "Home", new { id = bookId });
                return RedirectToAction("Login", "Account", new { returnUrl = back });
            }

            var book = db.Books.Find(bookId);
            if (book == null)
            {
                return HttpNotFound();
            }
            if (!book.IsActive || book.StockQuantity <= 0)
            {
                TempData["Error"] = "Sách \"" + book.Title + "\" hiện không còn hàng.";
                return RedirectBack(returnUrl);
            }

            var uid = CurrentUser.UserId;
            var item = db.CartItems.FirstOrDefault(c => c.UserId == uid && c.BookId == bookId);
            // Tính bằng kiểu long để không bị tràn số khi nhập số lượng rất lớn
            var wanted = (long)(item != null ? item.Quantity : 0) + Math.Max(1, Math.Min(quantity, MaxPerAdd));
            var finalQty = (int)Math.Min(wanted, book.StockQuantity);
            if (item == null)
            {
                db.CartItems.Add(new CartItem { UserId = uid, BookId = bookId, Quantity = finalQty, AddedAt = DateTime.Now });
            }
            else
            {
                item.Quantity = finalQty;
            }

            var error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectBack(returnUrl);
            }

            if (wanted > book.StockQuantity)
            {
                TempData["Error"] = "Chỉ còn " + book.StockQuantity + " cuốn \"" + book.Title + "\" trong kho. Giỏ hàng đã được cập nhật tối đa.";
            }
            else
            {
                TempData["Success"] = "Đã thêm \"" + book.Title + "\" vào giỏ hàng.";
            }
            // Sau khi thêm vào giỏ: chuyển tới trang giỏ hàng
            return buyNow || string.IsNullOrEmpty(returnUrl) ? RedirectToAction("Index") : RedirectBack(returnUrl);
        }

        // POST: Cart/UpdateQuantity
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int bookId, int quantity, int? setQuantity)
        {
            if (setQuantity.HasValue)
            {
                quantity = setQuantity.Value;   // nút +/- gửi setQuantity, ô nhập số gửi quantity
            }
            var uid = CurrentUser.UserId;
            var item = db.CartItems.Include(c => c.Book).FirstOrDefault(c => c.UserId == uid && c.BookId == bookId);
            if (item != null)
            {
                var stock = item.Book.StockQuantity;
                var qty = Math.Max(1, Math.Min(quantity, Math.Max(1, stock)));
                if (quantity > stock)
                {
                    TempData["Error"] = "Chỉ còn " + stock + " cuốn \"" + item.Book.Title + "\" trong kho.";
                }
                item.Quantity = qty;
                DbErrors.Save(db);
            }
            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveFromCart
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int bookId)
        {
            var uid = CurrentUser.UserId;
            var item = db.CartItems.FirstOrDefault(c => c.UserId == uid && c.BookId == bookId);
            if (item != null)
            {
                db.CartItems.Remove(item);
                DbErrors.Save(db);
            }
            return RedirectToAction("Index");
        }

        // POST: Cart/ClearCart
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            var uid = CurrentUser.UserId;
            db.CartItems.RemoveRange(db.CartItems.Where(c => c.UserId == uid));
            DbErrors.Save(db);
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
    }
}
