using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Controllers
{
    public class AccountController : Controller
    {
        private const string CustomerRole = "C";
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View(new RegisterVM());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                model.Username = model.Username.Trim();
                var username = model.Username;
                if (db.Users.Any(u => u.Username == username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                }
            }
            if (!string.IsNullOrWhiteSpace(model.CustomerPhone))
            {
                var phone = model.CustomerPhone.Trim();
                if (db.Customers.Any(c => c.CustomerPhone == phone))
                {
                    ModelState.AddModelError("CustomerPhone", "Số điện thoại đã được đăng ký");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 01 ban ghi User + 01 ban ghi Customer
            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                UserRole = CustomerRole
            };
            var customer = new Customer
            {
                CustomerName = model.CustomerName.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                CustomerEmail = model.CustomerEmail.Trim(),
                CustomerAddress = model.CustomerAddress.Trim(),
                Username = model.Username
            };
            db.Users.Add(user);
            db.Customers.Add(customer);
            var error = DbErrors.Save(db);
            if (error != null)
            {
                ModelState.AddModelError("", "Đăng ký không thành công: " + error);
                return View(model);
            }

            TempData["Success"] = "Đăng ký thành công! Mời bạn đăng nhập.";
            return RedirectToAction("Login", new { username = model.Username });
        }

        // GET: Account/Login
        public ActionResult Login(string returnUrl, string username)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginVM { ReturnUrl = returnUrl, Username = username });
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username.Trim();
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.UserRole == CustomerRole);
            if (user == null || user.Password.Trim() != model.Password)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
            Session["CustomerName"] = customer != null ? customer.CustomerName : user.Username;
            FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("CustomerName");
            Session.Remove("Cart");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/ProfileInfo
        [Authorize]
        public ActionResult ProfileInfo()
        {
            var customer = CurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Logout");
            }
            return View(new ProfileVM { Profile = ToProfile(customer) });
        }

        // POST: Account/UpdateProfile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile([Bind(Prefix = "Profile")] UpdateProfileVM model)
        {
            var customer = CurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Logout");
            }

            var phone = (model.CustomerPhone ?? "").Trim();
            if (db.Customers.Any(c => c.CustomerPhone == phone && c.CustomerID != customer.CustomerID))
            {
                ModelState.AddModelError("Profile.CustomerPhone", "Số điện thoại đã được tài khoản khác sử dụng");
            }
            if (!ModelState.IsValid)
            {
                model.Username = customer.Username;
                return View("ProfileInfo", new ProfileVM { Profile = model });
            }

            customer.CustomerName = model.CustomerName.Trim();
            customer.CustomerPhone = phone;
            customer.CustomerEmail = model.CustomerEmail.Trim();
            customer.CustomerAddress = model.CustomerAddress.Trim();
            db.SaveChanges();
            Session["CustomerName"] = customer.CustomerName;

            TempData["Success"] = "Đã cập nhật thông tin tài khoản";
            return RedirectToAction("ProfileInfo");
        }

        // POST: Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordVM model)
        {
            var customer = CurrentCustomer();
            var user = customer == null ? null : db.Users.Find(customer.Username);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            if (ModelState.IsValid && user.Password.Trim() != model.OldPassword)
            {
                ModelState.AddModelError("ChangePassword.OldPassword", "Mật khẩu hiện tại không đúng");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.ShowPasswordTab = true;
                return View("ProfileInfo", new ProfileVM { Profile = ToProfile(customer), ChangePassword = new ChangePasswordVM() });
            }

            user.Password = model.NewPassword;
            db.SaveChanges();
            TempData["Success"] = "Đã đổi mật khẩu";
            return RedirectToAction("ProfileInfo");
        }

        private Customer CurrentCustomer()
        {
            var username = User.Identity.Name;
            return db.Customers.FirstOrDefault(c => c.Username == username);
        }

        private static UpdateProfileVM ToProfile(Customer c)
        {
            return new UpdateProfileVM
            {
                Username = c.Username,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                CustomerEmail = c.CustomerEmail,
                CustomerAddress = c.CustomerAddress
            };
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
