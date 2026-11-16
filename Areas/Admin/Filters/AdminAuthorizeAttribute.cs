using System.Web.Mvc;
using System.Web.Routing;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Filters
{
    // Chi cho phep tai khoan co UserRole = "A" truy cap khu vuc Admin
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var role = session?["UserRole"] as string;

            if (session?["Username"] == null || role != "A")
            {
                var returnUrl = filterContext.HttpContext.Request.RawUrl;
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "area", "Admin" },
                    { "controller", "Account" },
                    { "action", "Login" },
                    { "returnUrl", returnUrl }
                });
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
