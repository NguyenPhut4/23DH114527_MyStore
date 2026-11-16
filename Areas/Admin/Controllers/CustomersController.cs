using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Quan ly khach hang: chi xem, tim kiem, sap xep, phan trang (khong Them/Sua/Xoa)
    [AdminAuthorize]
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index(CustomerSearchVM model, int? page)
        {
            var customers = db.Customers.Include(c => c.Orders).AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                var term = model.SearchTerm.Trim();
                customers = customers.Where(c => c.CustomerName.Contains(term)
                                              || c.CustomerPhone.Contains(term)
                                              || c.CustomerEmail.Contains(term)
                                              || c.Username.Contains(term));
            }

            switch (model.SortOrder)
            {
                case "name_desc": customers = customers.OrderByDescending(c => c.CustomerName); break;
                case "orders_desc": customers = customers.OrderByDescending(c => c.Orders.Count).ThenBy(c => c.CustomerName); break;
                case "orders_asc": customers = customers.OrderBy(c => c.Orders.Count).ThenBy(c => c.CustomerName); break;
                case "id_desc": customers = customers.OrderByDescending(c => c.CustomerID); break;
                default: customers = customers.OrderBy(c => c.CustomerName); break;
            }

            model.Customers = customers.ToPagedList(page ?? 1, model.PageSize);
            return View(model);
        }

        // GET: Admin/Customers/Details/5  (thong tin KH + danh sach don hang)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Customers
                             .Include(c => c.Orders)
                             .FirstOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
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
