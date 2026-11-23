using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class PublishersController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.BookCounts = db.Books.GroupBy(b => b.PublisherId).ToDictionary(g => g.Key, g => g.Count());
            return View(db.Publishers.OrderBy(p => p.PublisherName).ToList());
        }

        public ActionResult Create() { return View(new Publisher()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Publisher p)
        {
            if (ModelState.IsValid)
            {
                db.Publishers.Add(p);
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã thêm " + p.PublisherName; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            return View(p);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var p = db.Publishers.Find(id);
            return p == null ? (ActionResult)HttpNotFound() : View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Publisher input)
        {
            if (ModelState.IsValid)
            {
                var p = db.Publishers.Find(input.PublisherId);
                if (p == null) return HttpNotFound();
                p.PublisherName = input.PublisherName; p.Address = input.Address; p.Phone = input.Phone;
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã cập nhật " + p.PublisherName; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            return View(input);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var p = db.Publishers.Find(id);
            if (p == null) return HttpNotFound();
            ViewBag.BookCount = db.Books.Count(b => b.PublisherId == id);
            return View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var p = db.Publishers.Find(id);
            if (p == null) return HttpNotFound();
            if (db.Books.Any(b => b.PublisherId == id))
            {
                TempData["Error"] = "Không thể xóa \"" + p.PublisherName + "\" vì vẫn còn sách của nhà xuất bản này.";
                return RedirectToAction("Index");
            }
            db.Publishers.Remove(p);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa " + p.PublisherName;
            return RedirectToAction("Index");
        }
    }
}
