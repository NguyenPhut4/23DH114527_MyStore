using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using NguyenTanPhu_23DH114527.Models;

namespace NguyenTanPhu_23DH114527.Controllers
{
    // Controller gốc: giữ DbContext, tìm người dùng hiện tại, tự đăng xuất khi tài khoản bị khóa hoặc bị xóa
    public abstract class BaseController : Controller
    {
        protected HienSachEntities db = new HienSachEntities();

        private User currentUser;
        private bool currentUserLoaded;

        protected User CurrentUser
        {
            get
            {
                if (!currentUserLoaded)
                {
                    currentUserLoaded = true;
                    if (User.Identity.IsAuthenticated)
                    {
                        var name = User.Identity.Name;
                        currentUser = db.Users.FirstOrDefault(u => u.Username == name);
                    }
                }
                return currentUser;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated && (CurrentUser == null || !CurrentUser.IsActive))
            {
                // Tài khoản bị khóa hoặc không còn tồn tại: đăng xuất ngay
                FormsAuthentication.SignOut();
                if (!filterContext.IsChildAction)
                {
                    TempData["Error"] = "Tài khoản của bạn đã bị khóa hoặc không còn tồn tại.";
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "area", "" }, { "controller", "Account" }, { "action", "Login" } });
                }
                else
                {
                    filterContext.Result = new EmptyResult();
                }
                return;
            }
            base.OnActionExecuting(filterContext);
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
