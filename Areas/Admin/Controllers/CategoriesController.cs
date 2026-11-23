using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Danh mục 2 cấp (cha/con). Không xóa được danh mục còn sách hoặc còn danh mục con.
    [AdminAuthorize]
    public class CategoriesController : BaseController
    {
        public ActionResult Index()
        {
            var all = db.Categories.ToList();
            ViewBag.BookCounts = db.Books.GroupBy(b => b.CategoryId).ToDictionary(g => g.Key, g => g.Count());
            return View(all);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Categories.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        public ActionResult Create()
        {
            FillParents(null, 0);
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            Validate(category);
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã thêm danh mục " + category.CategoryName; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            FillParents(category.ParentId, 0);
            return View(category);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Categories.Find(id);
            if (c == null) return HttpNotFound();
            FillParents(c.ParentId, c.CategoryId);
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category input)
        {
            Validate(input);
            if (ModelState.IsValid)
            {
                var c = db.Categories.Find(input.CategoryId);
                if (c == null) return HttpNotFound();
                c.CategoryName = input.CategoryName; c.Description = input.Description; c.ParentId = input.ParentId;
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã cập nhật danh mục " + c.CategoryName; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            FillParents(input.ParentId, input.CategoryId);
            return View(input);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Categories.Find(id);
            if (c == null) return HttpNotFound();
            ViewBag.BookCount = db.Books.Count(b => b.CategoryId == id);
            ViewBag.ChildCount = db.Categories.Count(x => x.ParentId == id);
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var c = db.Categories.Find(id);
            if (c == null) return HttpNotFound();
            if (db.Books.Any(b => b.CategoryId == id))
            {
                TempData["Error"] = "Không thể xóa danh mục \"" + c.CategoryName + "\" vì vẫn còn sách thuộc danh mục này.";
                return RedirectToAction("Index");
            }
            if (db.Categories.Any(x => x.ParentId == id))
            {
                TempData["Error"] = "Không thể xóa danh mục \"" + c.CategoryName + "\" vì vẫn còn danh mục con.";
                return RedirectToAction("Index");
            }
            db.Categories.Remove(c);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa danh mục " + c.CategoryName;
            return RedirectToAction("Index");
        }

        private void Validate(Category c)
        {
            if (!string.IsNullOrWhiteSpace(c.CategoryName) &&
                db.Categories.Any(x => x.CategoryName == c.CategoryName && x.ParentId == c.ParentId && x.CategoryId != c.CategoryId))
                ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại");
            if (c.ParentId != null)
            {
                if (c.ParentId == c.CategoryId) ModelState.AddModelError("ParentId", "Danh mục không thể là cha của chính nó");
                var parent = db.Categories.Find(c.ParentId);
                if (parent == null || parent.ParentId != null) ModelState.AddModelError("ParentId", "Chỉ hỗ trợ 2 cấp: danh mục cha phải là danh mục gốc");
                if (c.CategoryId != 0 && db.Categories.Any(x => x.ParentId == c.CategoryId))
                    ModelState.AddModelError("ParentId", "Danh mục đang có danh mục con nên không thể chuyển thành danh mục con");
            }
        }

        private void FillParents(int? selected, int exceptId)
        {
            ViewBag.ParentId = new SelectList(db.Categories.Where(c => c.ParentId == null && c.CategoryId != exceptId).OrderBy(c => c.CategoryName),
                                              "CategoryId", "CategoryName", selected);
        }
    }
}
