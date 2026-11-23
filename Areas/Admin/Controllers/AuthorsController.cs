using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Tác giả và dịch giả (cùng một bảng Authors)
    [AdminAuthorize]
    public class AuthorsController : BaseController
    {
        public ActionResult Index(string q, int? page)
        {
            var authors = db.Authors.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                authors = authors.Where(a => a.AuthorName.Contains(t) || a.Nationality.Contains(t));
            }
            ViewBag.Q = q;
            return View(authors.OrderBy(a => a.AuthorName).ToPagedList(BookQueries.Page(page), 10));
        }

        public ActionResult Create() { return View(new Author()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Author author)
        {
            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã thêm " + author.AuthorName; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            return View(author);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var a = db.Authors.Find(id);
            return a == null ? (ActionResult)HttpNotFound() : View(a);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Author input)
        {
            if (ModelState.IsValid)
            {
                var a = db.Authors.Find(input.AuthorId);
                if (a == null) return HttpNotFound();
                a.AuthorName = input.AuthorName; a.BirthYear = input.BirthYear; a.Nationality = input.Nationality; a.Biography = input.Biography;
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã cập nhật " + a.AuthorName; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            return View(input);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var a = db.Authors.Find(id);
            if (a == null) return HttpNotFound();
            ViewBag.BookCount = db.BookAuthors.Count(x => x.AuthorId == id);
            return View(a);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var a = db.Authors.Find(id);
            if (a == null) return HttpNotFound();
            if (db.BookAuthors.Any(x => x.AuthorId == id))
            {
                TempData["Error"] = "Không thể xóa \"" + a.AuthorName + "\" vì còn sách liên quan.";
                return RedirectToAction("Index");
            }
            db.Authors.Remove(a);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa " + a.AuthorName;
            return RedirectToAction("Index");
        }
    }
}
