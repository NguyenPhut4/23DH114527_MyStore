using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Tài khoản quản trị và nhân viên (Admin / Staff): chỉ Admin quản lý
    [AdminAuthorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        public ActionResult Index(UserSearchVM model, int? page)
        {
            var users = db.Users.Where(u => u.Role != UserRoles.Customer);
            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                var t = model.SearchTerm.Trim();
                users = users.Where(u => u.FullName.Contains(t) || u.Username.Contains(t) || u.Email.Contains(t));
            }
            model.Users = users.OrderBy(u => u.Role).ThenBy(u => u.Username).ToPagedList(BookQueries.Page(page), model.PageSize);
            return View(model);
        }

        public ActionResult Create()
        {
            FillRoles(UserRoles.Staff);
            return View(new UserFormVM { Role = UserRoles.Staff });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserFormVM vm)
        {
            if (string.IsNullOrEmpty(vm.Password)) ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu");
            CheckUnique(vm);
            CheckRole(vm);
            if (ModelState.IsValid)
            {
                db.Users.Add(new User
                {
                    Username = vm.Username, FullName = vm.FullName, Email = vm.Email, PhoneNumber = vm.PhoneNumber, Address = vm.Address,
                    Role = vm.Role, IsActive = vm.IsActive, CreatedAt = System.DateTime.Now, PasswordHash = Auth.HashPassword(vm.Password)
                });
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã tạo tài khoản " + vm.Username; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            FillRoles(vm.Role);
            return View(vm);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var u = db.Users.FirstOrDefault(x => x.UserId == id && x.Role != UserRoles.Customer);
            if (u == null) return HttpNotFound();
            FillRoles(u.Role);
            return View(new UserFormVM
            {
                UserId = u.UserId, Username = u.Username, FullName = u.FullName, Email = u.Email, PhoneNumber = u.PhoneNumber,
                Address = u.Address, Role = u.Role, IsActive = u.IsActive
            });
        }

        // Mật khẩu để trống = giữ nguyên. Không cho tự khóa hoặc hạ quyền chính mình.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserFormVM vm)
        {
            var u = db.Users.FirstOrDefault(x => x.UserId == vm.UserId && x.Role != UserRoles.Customer);
            if (u == null) return HttpNotFound();
            CheckUnique(vm);
            CheckRole(vm);
            if (CurrentUser != null && CurrentUser.UserId == u.UserId && (!vm.IsActive || vm.Role != u.Role))
                ModelState.AddModelError("", "Bạn không thể khóa hoặc đổi vai trò của chính mình.");
            if (ModelState.IsValid)
            {
                u.Username = vm.Username; u.FullName = vm.FullName; u.Email = vm.Email; u.PhoneNumber = vm.PhoneNumber;
                u.Address = vm.Address; u.Role = vm.Role; u.IsActive = vm.IsActive;
                if (!string.IsNullOrEmpty(vm.Password)) u.PasswordHash = Auth.HashPassword(vm.Password);
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã cập nhật tài khoản " + u.Username; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            FillRoles(vm.Role);
            return View(vm);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var u = db.Users.FirstOrDefault(x => x.UserId == id && x.Role != UserRoles.Customer);
            return u == null ? (ActionResult)HttpNotFound() : View(u);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var u = db.Users.FirstOrDefault(x => x.UserId == id && x.Role != UserRoles.Customer);
            if (u == null) return HttpNotFound();
            if (CurrentUser != null && CurrentUser.UserId == id)
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction("Index");
            }
            if (db.Orders.Any(o => o.UserId == id) || db.Reviews.Any(r => r.UserId == id))
            {
                TempData["Error"] = "Tài khoản " + u.Username + " đã phát sinh dữ liệu nên không thể xóa. Hãy khóa tài khoản.";
                return RedirectToAction("Index");
            }
            db.CartItems.RemoveRange(db.CartItems.Where(c => c.UserId == id));
            db.Users.Remove(u);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa tài khoản " + u.Username;
            return RedirectToAction("Index");
        }

        private void CheckUnique(UserFormVM vm)
        {
            if (!string.IsNullOrEmpty(vm.Username) && db.Users.Any(x => x.Username == vm.Username && x.UserId != vm.UserId))
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            if (!string.IsNullOrEmpty(vm.Email) && db.Users.Any(x => x.Email == vm.Email && x.UserId != vm.UserId))
                ModelState.AddModelError("Email", "Email đã được sử dụng");
        }

        private void CheckRole(UserFormVM vm)
        {
            if (vm.Role != UserRoles.Admin && vm.Role != UserRoles.Staff)
                ModelState.AddModelError("Role", "Vai trò phải là Admin hoặc Staff");
        }

        private void FillRoles(string selected)
        {
            ViewBag.Roles = new SelectList(new[] { UserRoles.Admin, UserRoles.Staff }, selected);
        }
    }
}
