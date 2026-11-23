using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Quản lý sách: tìm kiếm, sắp xếp, phân trang, thêm/sửa/xóa, tải ảnh bìa, gán tác giả và dịch giả
    [AdminAuthorize]
    public class BooksController : BaseController
    {
        private static readonly string[] ImageExt = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

        public ActionResult Index(BookSearchVM model, int? page)
        {
            var books = db.Books.Include(b => b.Category).Include(b => b.Publisher)
                          .Include(b => b.BookAuthors.Select(x => x.Author)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                var term = model.SearchTerm.Trim();
                books = books.Where(b => b.Title.Contains(term) || b.ISBN.Contains(term) || b.BookAuthors.Any(x => x.Author.AuthorName.Contains(term)));
            }
            if (model.CategoryId.HasValue)
            {
                var id = model.CategoryId.Value;
                books = books.Where(b => b.CategoryId == id || b.Category.ParentId == id);
            }
            if (model.MinPrice.HasValue) { var v = model.MinPrice.Value; books = books.Where(b => b.Price >= v); }
            if (model.MaxPrice.HasValue) { var v = model.MaxPrice.Value; books = books.Where(b => b.Price <= v); }
            if (model.StockFilter == "out") books = books.Where(b => b.StockQuantity == 0);
            else if (model.StockFilter == "low") books = books.Where(b => b.StockQuantity <= 10);
            else if (model.StockFilter == "inactive") books = books.Where(b => !b.IsActive);

            switch (model.SortOrder)
            {
                case "title_desc": books = books.OrderByDescending(b => b.Title); break;
                case "price_asc": books = books.OrderBy(b => b.Price); break;
                case "price_desc": books = books.OrderByDescending(b => b.Price); break;
                case "stock_asc": books = books.OrderBy(b => b.StockQuantity); break;
                case "stock_desc": books = books.OrderByDescending(b => b.StockQuantity); break;
                case "date_desc": books = books.OrderByDescending(b => b.CreatedAt); break;
                default: books = books.OrderBy(b => b.Title); break;
            }

            ViewBag.Categories = CategorySelect(model.CategoryId);
            model.Books = books.ToPagedList(BookQueries.Page(page), model.PageSize);
            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var book = db.Books.Include(b => b.Category).Include(b => b.Publisher)
                         .Include(b => b.BookAuthors.Select(x => x.Author)).FirstOrDefault(b => b.BookId == id);
            if (book == null) return HttpNotFound();
            ViewBag.SoldCount = db.OrderItems.Where(oi => oi.BookId == id && oi.Order.Status != OrderStatuses.Cancelled).Select(oi => (int?)oi.Quantity).Sum() ?? 0;
            return View(book);
        }

        public ActionResult Create()
        {
            var book = new Book { IsActive = true, StockQuantity = 0, PublishYear = DateTime.Today.Year };
            FillLists(book, null, null);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Book book, int[] authorIds, int[] translatorIds)
        {
            ValidateBook(book);
            if (ModelState.IsValid)
            {
                book.CreatedAt = DateTime.Now;
                book.CoverImage = SaveImage(book.UploadImg) ?? book.CoverImage;
                db.Books.Add(book);
                SetAuthors(book, authorIds, translatorIds);
                var error = DbErrors.Save(db);
                if (error == null)
                {
                    TempData["Success"] = "Đã thêm sách " + book.Title;
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", error);
            }
            FillLists(book, authorIds, translatorIds);
            return View(book);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var book = db.Books.Include(b => b.BookAuthors).FirstOrDefault(b => b.BookId == id);
            if (book == null) return HttpNotFound();
            FillLists(book, book.BookAuthors.Where(x => x.AuthorRole != "Dịch giả").Select(x => x.AuthorId).ToArray(),
                            book.BookAuthors.Where(x => x.AuthorRole == "Dịch giả").Select(x => x.AuthorId).ToArray());
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Book input, int[] authorIds, int[] translatorIds)
        {
            ValidateBook(input);
            var book = db.Books.Include(b => b.BookAuthors).FirstOrDefault(b => b.BookId == input.BookId);
            if (book == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                book.ISBN = input.ISBN; book.Title = input.Title; book.CategoryId = input.CategoryId; book.PublisherId = input.PublisherId;
                book.Price = input.Price; book.StockQuantity = input.StockQuantity; book.PageCount = input.PageCount;
                book.PublishYear = input.PublishYear; book.Description = input.Description; book.IsActive = input.IsActive;
                var uploaded = SaveImage(input.UploadImg);
                if (uploaded != null) book.CoverImage = uploaded;

                foreach (var old in book.BookAuthors.ToList()) db.BookAuthors.Remove(old);
                SetAuthors(book, authorIds, translatorIds);

                var error = DbErrors.Save(db);
                if (error == null)
                {
                    TempData["Success"] = "Đã cập nhật sách " + book.Title;
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", error);
                db.Entry(book).Reload();
            }
            input.CoverImage = book.CoverImage;
            FillLists(input, authorIds, translatorIds);
            return View(input);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var book = db.Books.Include(b => b.Category).Include(b => b.Publisher).FirstOrDefault(b => b.BookId == id);
            if (book == null) return HttpNotFound();
            ViewBag.OrderCount = db.OrderItems.Count(oi => oi.BookId == id);
            return View(book);
        }

        // Sách đã có trong đơn hàng thì không xóa được: chuyển sang ngừng kinh doanh
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var book = db.Books.Include(b => b.BookAuthors).Include(b => b.Reviews).Include(b => b.CartItems).FirstOrDefault(b => b.BookId == id);
            if (book == null) return HttpNotFound();
            if (db.OrderItems.Any(oi => oi.BookId == id))
            {
                TempData["Error"] = "Sách \"" + book.Title + "\" đã có trong đơn hàng nên không thể xóa. Hãy bỏ chọn \"Đang kinh doanh\" để ngừng bán.";
                return RedirectToAction("Index");
            }
            foreach (var x in book.BookAuthors.ToList()) db.BookAuthors.Remove(x);
            foreach (var x in book.Reviews.ToList()) db.Reviews.Remove(x);
            foreach (var x in book.CartItems.ToList()) db.CartItems.Remove(x);
            db.BookPriceHistories.RemoveRange(db.BookPriceHistories.Where(h => h.BookId == id));
            db.Books.Remove(book);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa sách " + book.Title;
            return RedirectToAction("Index");
        }

        // Kho: tồn kho và hàng cần nhập (tồn <= 10)
        public ActionResult Stock(string filter, int? page)
        {
            var books = db.Books.Include(b => b.Category).AsQueryable();
            if (filter != "all") books = books.Where(b => b.StockQuantity <= 10);
            ViewBag.Filter = filter == "all" ? "all" : "low";
            return View(books.OrderBy(b => b.StockQuantity).ThenBy(b => b.Title).ToPagedList(BookQueries.Page(page), 15));
        }

        private void ValidateBook(Book book)
        {
            if (!string.IsNullOrWhiteSpace(book.ISBN) && db.Books.Any(b => b.ISBN == book.ISBN && b.BookId != book.BookId))
                ModelState.AddModelError("ISBN", "Mã ISBN này đã tồn tại");
            var f = book.UploadImg;
            if (f != null && f.ContentLength > 0)
            {
                if (!ImageExt.Contains(Path.GetExtension(f.FileName ?? "").ToLowerInvariant()))
                    ModelState.AddModelError("UploadImg", "Chỉ nhận ảnh .jpg, .png, .gif, .webp, .svg");
                else if (f.ContentLength > 2 * 1024 * 1024)
                    ModelState.AddModelError("UploadImg", "Ảnh tối đa 2 MB");
            }
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var dir = Server.MapPath("~/Content/images/books");
            Directory.CreateDirectory(dir);
            var name = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, name));
            return "~/Content/images/books/" + name;
        }

        private void SetAuthors(Book book, int[] authorIds, int[] translatorIds)
        {
            var added = new HashSet<int>();
            foreach (var id in (authorIds ?? new int[0]).Distinct().Select((v, i) => new { v, i }))
            {
                if (added.Add(id.v))
                    book.BookAuthors.Add(new BookAuthor { AuthorId = id.v, AuthorRole = id.i == 0 ? "Tác giả" : "Đồng tác giả" });
            }
            foreach (var id in (translatorIds ?? new int[0]).Distinct())
            {
                if (added.Add(id))
                    book.BookAuthors.Add(new BookAuthor { AuthorId = id, AuthorRole = "Dịch giả" });
            }
        }

        private void FillLists(Book book, int[] authorIds, int[] translatorIds)
        {
            ViewBag.CategoryId = CategorySelect(book.CategoryId == 0 ? (int?)null : book.CategoryId, true);
            ViewBag.PublisherId = new SelectList(db.Publishers.OrderBy(p => p.PublisherName), "PublisherId", "PublisherName", book.PublisherId);
            var authors = db.Authors.OrderBy(a => a.AuthorName).ToList();
            ViewBag.AuthorIds = new MultiSelectList(authors, "AuthorId", "AuthorName", authorIds);
            ViewBag.TranslatorIds = new MultiSelectList(authors, "AuthorId", "AuthorName", translatorIds);
        }

        // Danh mục dạng 2 cấp: "Cha" và "Cha › Con"
        private SelectList CategorySelect(int? selected, bool leafOnly = false)
        {
            var all = db.Categories.ToList();
            var items = new List<object>();
            foreach (var root in all.Where(c => c.ParentId == null).OrderBy(c => c.CategoryName))
            {
                var kids = all.Where(c => c.ParentId == root.CategoryId).OrderBy(c => c.CategoryName).ToList();
                if (!leafOnly || kids.Count == 0) items.Add(new { Value = root.CategoryId, Text = root.CategoryName });
                foreach (var k in kids) items.Add(new { Value = k.CategoryId, Text = root.CategoryName + " › " + k.CategoryName });
            }
            return new SelectList(items, "Value", "Text", selected);
        }
    }
}
