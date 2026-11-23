using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Kiểm duyệt đánh giá: duyệt (hiển thị) hoặc xóa
    [AdminAuthorize]
    public class ReviewsController : BaseController
    {
        public ActionResult Index(string status, int? page)
        {
            var reviews = db.Reviews.Include(r => r.Book).Include(r => r.User).AsQueryable();
            if (status == "pending") reviews = reviews.Where(r => !r.IsApproved);
            else if (status == "approved") reviews = reviews.Where(r => r.IsApproved);
            ViewBag.Status = status;
            return View(reviews.OrderBy(r => r.IsApproved).ThenByDescending(r => r.CreatedAt).ToPagedList(BookQueries.Page(page), 15));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Toggle(int id, string status)
        {
            var r = db.Reviews.Find(id);
            if (r == null) return HttpNotFound();
            r.IsApproved = !r.IsApproved;
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? (r.IsApproved ? "Đã duyệt đánh giá" : "Đã ẩn đánh giá");
            return RedirectToAction("Index", new { status });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string status)
        {
            var r = db.Reviews.Find(id);
            if (r == null) return HttpNotFound();
            db.Reviews.Remove(r);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa đánh giá";
            return RedirectToAction("Index", new { status });
        }
    }
}
