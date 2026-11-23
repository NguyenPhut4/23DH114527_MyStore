using System.Net;
using System.Web.Mvc;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Filters
{
    // Chỉ cho Admin và Staff vào khu vực quản trị (dùng Roles = "Admin" cho chức năng chỉ dành cho Admin).
    // Chưa đăng nhập: chuyển tới trang đăng nhập. Đã đăng nhập nhưng sai vai trò: trả về 403.
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            Roles = "Admin,Staff";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new ViewResult { ViewName = "~/Views/Shared/AccessDenied.cshtml" };
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
