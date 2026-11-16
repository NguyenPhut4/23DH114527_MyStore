using System.Web;
using System.Web.Mvc;

namespace NguyenTanPhu_23DH114527
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
