using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Account/Login
        public ActionResult Login(string returnUrl)
        {
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        // POST: Admin/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username.Trim();
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.UserRole == "A");

            if (user == null || user.Password.Trim() != model.Password)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole.Trim();

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // GET: Admin/Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
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
