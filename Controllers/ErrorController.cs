using System.Web.Mvc;

namespace NguyenTanPhu_23DH114527.Controllers
{
    // Trang lỗi thân thiện (cấu hình trong Web.config > customErrors)
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        public ActionResult Server()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }
    }
}
