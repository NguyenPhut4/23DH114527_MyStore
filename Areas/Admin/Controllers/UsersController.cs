using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Quan ly tai khoan Admin (UserRole = "A"): CRUD, tim kiem, phan trang
    [AdminAuthorize]
    public class UsersController : Controller
    {
        private const string AdminRole = "A";
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Users
        public ActionResult Index(UserSearchVM model, int? page)
        {
            var users = db.Users.Where(u => u.UserRole == AdminRole);
            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                var term = model.SearchTerm.Trim();
                users = users.Where(u => u.Username.Contains(term));
            }
            model.Users = users.OrderBy(u => u.Username).ToPagedList(page ?? 1, model.PageSize);
            return View(model);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            return View(new User());
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password")] User user, string confirmPassword)
        {
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                user.Username = user.Username.Trim();
                var name = user.Username;
                if (db.Users.Any(u => u.Username == name))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                }
            }
            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");
            }

            if (ModelState.IsValid)
            {
                user.UserRole = AdminRole;
                db.Users.Add(user);
                db.SaveChanges();
                TempData["Success"] = "Đã tạo tài khoản " + user.Username;
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/admin  (doi mat khau)
        public ActionResult Edit(string id)
        {
            return FindAdmin(id, "Edit");
        }

        // POST: Admin/Users/Edit/admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password")] User user, string confirmPassword)
        {
            var existing = db.Users.FirstOrDefault(u => u.Username == user.Username && u.UserRole == AdminRole);
            if (existing == null)
            {
                return HttpNotFound();
            }
            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");
            }

            if (ModelState.IsValid)
            {
                existing.Password = user.Password;
                db.SaveChanges();
                TempData["Success"] = "Đã đổi mật khẩu cho " + existing.Username;
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Users/Delete/admin
        public ActionResult Delete(string id)
        {
            return FindAdmin(id, "Delete");
        }

        // POST: Admin/Users/Delete/admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == id && u.UserRole == AdminRole);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (user.Username == (string)Session["Username"])
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }
            if (db.Users.Count(u => u.UserRole == AdminRole) <= 1)
            {
                TempData["Error"] = "Phải còn ít nhất một tài khoản Admin.";
                return RedirectToAction("Index");
            }
            if (db.Customers.Any(c => c.Username == id))
            {
                TempData["Error"] = "Tài khoản đang gắn với hồ sơ khách hàng, không thể xóa.";
                return RedirectToAction("Index");
            }

            db.Users.Remove(user);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa tài khoản " + id;
            return RedirectToAction("Index");
        }

        private ActionResult FindAdmin(string id, string viewName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.FirstOrDefault(u => u.Username == id && u.UserRole == AdminRole);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (viewName == "Edit")
            {
                user.Password = ""; // khong hien mat khau cu
            }
            return View(viewName, user);
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
