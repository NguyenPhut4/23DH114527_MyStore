using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account/Register
        public ActionResult Register()
        {
            return View(new RegisterVM());
        }

        // POST: Account/Register  (tạo tài khoản Customer, mật khẩu băm BCrypt)
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
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                model.Email = model.Email.Trim();
                var email = model.Email;
                if (db.Users.Any(u => u.Email == email))
                {
                    ModelState.AddModelError("Email", "Email đã được đăng ký");
                }
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            db.Users.Add(new User
            {
                Username = model.Username,
                PasswordHash = Auth.HashPassword(model.Password),
                FullName = model.FullName.Trim(),
                Email = model.Email,
                PhoneNumber = model.PhoneNumber.Trim(),
                Address = model.Address.Trim(),
                Role = UserRoles.Customer,
                CreatedAt = DateTime.Now,
                IsActive = true
            });
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
                return RedirectAfterLogin(returnUrl);
            }
            return View(new LoginVM { ReturnUrl = returnUrl, Username = username });
        }

        // POST: Account/Login  (một trang đăng nhập cho cả khách hàng, nhân viên và quản trị)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username.Trim();
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !Auth.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }
            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            Auth.SignIn(Response, user, model.RememberMe);
            return RedirectAfterLogin(model.ReturnUrl, user.Role);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // GET: Account/AccessDenied
        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        // GET: Account/ProfileInfo
        [Authorize]
        public ActionResult ProfileInfo()
        {
            return View(new ProfileVM { Profile = ToProfile(CurrentUser) });
        }

        // POST: Account/UpdateProfile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile([Bind(Prefix = "Profile")] UpdateProfileVM model)
        {
            var user = CurrentUser;
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var email = model.Email.Trim();
                if (db.Users.Any(u => u.Email == email && u.UserId != user.UserId))
                {
                    ModelState.AddModelError("Profile.Email", "Email đã được tài khoản khác sử dụng");
                }
            }
            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                return View("ProfileInfo", new ProfileVM { Profile = model });
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Email = model.Email.Trim();
            user.Address = model.Address.Trim();
            var error = DbErrors.Save(db);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                model.Username = user.Username;
                return View("ProfileInfo", new ProfileVM { Profile = model });
            }

            TempData["Success"] = "Đã cập nhật thông tin tài khoản";
            return RedirectToAction("ProfileInfo");
        }

        // POST: Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordVM model)
        {
            var user = CurrentUser;
            if (ModelState.IsValid && !Auth.Verify(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("ChangePassword.OldPassword", "Mật khẩu hiện tại không đúng");
            }
            if (!ModelState.IsValid)
            {
                ViewBag.ShowPasswordTab = true;
                return View("ProfileInfo", new ProfileVM { Profile = ToProfile(user), ChangePassword = new ChangePasswordVM() });
            }

            user.PasswordHash = Auth.HashPassword(model.NewPassword);
            var error = DbErrors.Save(db);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("ProfileInfo");
            }
            TempData["Success"] = "Đã đổi mật khẩu";
            return RedirectToAction("ProfileInfo");
        }

        private ActionResult RedirectAfterLogin(string returnUrl, string role = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            role = role ?? (User.IsInRole(UserRoles.Admin) ? UserRoles.Admin : User.IsInRole(UserRoles.Staff) ? UserRoles.Staff : UserRoles.Customer);
            if (role == UserRoles.Admin || role == UserRoles.Staff)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        private static UpdateProfileVM ToProfile(User u)
        {
            return new UpdateProfileVM
            {
                Username = u.Username,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                Address = u.Address
            };
        }
    }
}
