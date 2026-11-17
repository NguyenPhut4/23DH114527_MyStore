using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Models;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CategoriesController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Categories
        public ActionResult Index()
        {
            var categories = db.Categories.Include(c => c.Products).OrderBy(c => c.CategoryName).ToList();
            return View(categories);
        }

        // GET: Admin/Categories/Details/5
        public ActionResult Details(int? id)
        {
            return FindCategory(id, "Details");
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View(new Category());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            CheckDuplicateName(category);
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                var error = DbErrors.Save(db);
                if (error == null)
                {
                    TempData["Success"] = "Đã thêm danh mục \"" + category.CategoryName + "\"";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", error);
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            return FindCategory(id, "Edit");
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            CheckDuplicateName(category);
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                var error = DbErrors.Save(db);
                if (error == null)
                {
                    TempData["Success"] = "Đã cập nhật danh mục";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", error);
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            return FindCategory(id, "Delete");
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Kiem tra rang buoc: khong xoa danh muc con san pham
            if (category.Products.Any())
            {
                TempData["Error"] = "Không thể xóa danh mục \"" + category.CategoryName + "\" vì còn "
                                    + category.Products.Count + " sản phẩm thuộc danh mục này.";
                return RedirectToAction("Delete", new { id });
            }

            db.Categories.Remove(category);
            var error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Delete", new { id });
            }
            TempData["Success"] = "Đã xóa danh mục";
            return RedirectToAction("Index");
        }

        // Ham dung chung cho Details / Edit / Delete (GET)
        private ActionResult FindCategory(int? id, string viewName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(viewName, category);
        }

        private void CheckDuplicateName(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                return;
            }
            category.CategoryName = category.CategoryName.Trim();
            var name = category.CategoryName;
            if (db.Categories.Any(c => c.CategoryName == name && c.CategoryID != category.CategoryID))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại");
            }
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
