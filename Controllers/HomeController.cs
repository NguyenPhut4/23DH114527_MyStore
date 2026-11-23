using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home/Index?searchTerm=&page=
        public ActionResult Index(string searchTerm, int? page)
        {
            var books = BookQueries.Available(db).Search(searchTerm);

            var vm = new HomeBookVM
            {
                SearchTerm = searchTerm,
                // Top 10 sách bán chạy nhất
                FeaturedBooks = BookQueries.FillSold(db, books.WithAuthors().OrderByBestSelling().Take(10).ToList()).ToList(),
                // Sách mới nhất (24 cuốn), 8 cuốn mỗi trang
                NewBooks = BookQueries.FillSold(db, books.WithAuthors().OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.BookId).Take(24).ToList())
                                      .ToPagedList(BookQueries.Page(page), 8)
            };
            return View(vm);
        }

        // GET: Home/ProductList/5  (sách theo danh mục, gồm cả danh mục con)
        public ActionResult ProductList(int? id, string searchTerm, string sortOrder, decimal? minPrice, decimal? maxPrice, int? page)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var category = db.Categories.Include(c => c.ChildCategories).FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var categoryIds = new List<int> { category.CategoryId };
            categoryIds.AddRange(category.ChildCategories.Select(c => c.CategoryId));
            var books = BookQueries.Available(db).Where(b => categoryIds.Contains(b.CategoryId)).Search(searchTerm);

            if (minPrice.HasValue)
            {
                var min = minPrice.Value;
                books = books.Where(b => b.Price >= min);
            }
            if (maxPrice.HasValue)
            {
                var max = maxPrice.Value;
                books = books.Where(b => b.Price <= max);
            }

            switch (sortOrder)
            {
                case "name_asc": books = books.OrderBy(b => b.Title); break;
                case "name_desc": books = books.OrderByDescending(b => b.Title); break;
                case "price_asc": books = books.OrderBy(b => b.Price); break;
                case "price_desc": books = books.OrderByDescending(b => b.Price); break;
                default: books = books.OrderByBestSelling(); break;
            }

            var paged = books.WithAuthors().ToPagedList(BookQueries.Page(page), 9);
            BookQueries.FillSold(db, paged);
            var vm = new BookListVM
            {
                Category = category,
                Children = category.ChildCategories.OrderBy(c => c.CategoryName).ToList(),
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Books = paged
            };
            return View(vm);
        }

        // GET: Home/ProductDetail/5?quantity=1&page=1
        public ActionResult ProductDetail(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var book = db.Books.Include(b => b.Category.ParentCategory).Include(b => b.Publisher)
                               .Include(b => b.BookAuthors.Select(a => a.Author))
                               .FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                return HttpNotFound();
            }
            BookQueries.FillSold(db, new[] { book });

            var approved = db.Reviews.Where(r => r.BookId == book.BookId && r.IsApproved);
            var vm = new BookDetailVM
            {
                Book = book,
                Quantity = quantity.HasValue && quantity.Value > 0 ? quantity.Value : 1,
                ReviewCount = approved.Count(),
                AvgRating = approved.Select(r => (double?)r.Rating).Average() ?? 0,
                Reviews = approved.Include(r => r.User).OrderByDescending(r => r.CreatedAt).Take(20).ToList()
            };

            if (CurrentUser != null)
            {
                var uid = CurrentUser.UserId;
                vm.MyReview = db.Reviews.FirstOrDefault(r => r.BookId == book.BookId && r.UserId == uid);
                // Chỉ được đánh giá sách đã mua và đã nhận hàng
                vm.CanReview = db.OrderItems.Any(oi => oi.BookId == book.BookId && oi.Order.UserId == uid && oi.Order.Status == OrderStatuses.Completed);
            }

            var sameCategory = BookQueries.Available(db).Where(b => b.CategoryId == book.CategoryId && b.BookId != book.BookId);
            vm.RelatedBooks = BookQueries.FillSold(db, sameCategory.WithAuthors().OrderByDescending(b => b.BookId).Take(8).ToList()).ToList();
            var top = sameCategory.WithAuthors().OrderByBestSelling().Take(8).ToList();
            BookQueries.FillSold(db, top);
            vm.TopBooks = top.ToPagedList(BookQueries.Page(page), 3);
            return View(vm);
        }

        // POST: Home/AddReview  (chỉ cho khách đã nhận sách; đánh giá mới chờ Admin duyệt)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddReview(int bookId, int rating, string comment)
        {
            var uid = CurrentUser.UserId;
            var purchased = db.OrderItems.Any(oi => oi.BookId == bookId && oi.Order.UserId == uid && oi.Order.Status == OrderStatuses.Completed);
            if (!purchased)
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá sách đã mua và đã nhận hàng.";
                return RedirectToAction("ProductDetail", new { id = bookId });
            }
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Điểm đánh giá phải từ 1 đến 5 sao.";
                return RedirectToAction("ProductDetail", new { id = bookId });
            }
            comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
            if (comment != null && comment.Length > 1000)
            {
                comment = comment.Substring(0, 1000);
            }

            var review = db.Reviews.FirstOrDefault(r => r.BookId == bookId && r.UserId == uid);
            if (review == null)
            {
                db.Reviews.Add(new Review { BookId = bookId, UserId = uid, Rating = rating, Comment = comment, CreatedAt = DateTime.Now, IsApproved = false });
            }
            else
            {
                review.Rating = rating;
                review.Comment = comment;
                review.CreatedAt = DateTime.Now;
                review.IsApproved = false;
            }
            var error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Cảm ơn bạn! Đánh giá đang chờ quản trị viên duyệt.";
            }
            return RedirectToAction("ProductDetail", new { id = bookId });
        }

        // Menu danh mục trên header (2 cấp)
        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var roots = db.Categories.Include(c => c.ChildCategories).Where(c => c.ParentId == null).OrderBy(c => c.CategoryName).ToList();
            return PartialView("_CategoriesPV", roots);
        }

        // Menu cá nhân trên header: Trang chủ, Tài khoản, Giỏ hàng
        [ChildActionOnly]
        public ActionResult PersonalMenu()
        {
            var user = CurrentUser;
            var vm = new PersonalMenuVM { IsLoggedIn = user != null };
            if (user != null)
            {
                var uid = user.UserId;
                vm.Username = user.Username;
                vm.DisplayName = user.FullName;
                vm.Role = user.Role;
                vm.CartCount = db.CartItems.Where(c => c.UserId == uid).Sum(c => (int?)c.Quantity) ?? 0;
            }
            return PartialView("_PersonalMenuPV", vm);
        }
    }
}
