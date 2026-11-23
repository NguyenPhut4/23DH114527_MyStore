using System.Data.Entity;
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
    // Khách hàng (Users có Role = Customer): xem, tìm kiếm, sắp xếp, phân trang; khóa hoặc mở khóa (chỉ Admin)
    [AdminAuthorize]
    public class CustomersController : BaseController
    {
        // GET: Admin/Customers
        public ActionResult Index(UserSearchVM model, int? page)
        {
            var customers = db.Users.Where(u => u.Role == UserRoles.Customer);
            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                var term = model.SearchTerm.Trim();
                customers = customers.Where(u => u.FullName.Contains(term) || u.Username.Contains(term) || u.Email.Contains(term) || u.PhoneNumber.Contains(term));
            }

            switch (model.SortOrder)
            {
                case "name_desc": customers = customers.OrderByDescending(u => u.FullName); break;
                case "orders_desc": customers = customers.OrderByDescending(u => u.Orders.Count).ThenBy(u => u.FullName); break;
                case "orders_asc": customers = customers.OrderBy(u => u.Orders.Count).ThenBy(u => u.FullName); break;
                case "date_desc": customers = customers.OrderByDescending(u => u.CreatedAt); break;
                default: customers = customers.OrderBy(u => u.FullName); break;
            }

            model.Users = customers.ToPagedList(BookQueries.Page(page), model.PageSize);
            return View(model);
        }

        // GET: Admin/Customers/Details/5  (thông tin khách hàng + danh sách đơn hàng)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Users.Include(u => u.Orders).FirstOrDefault(u => u.UserId == id && u.Role == UserRoles.Customer);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/ToggleActive/5  (khóa hoặc mở khóa; khách bị khóa sẽ bị đăng xuất ngay)
        [AdminAuthorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var customer = db.Users.FirstOrDefault(u => u.UserId == id && u.Role == UserRoles.Customer);
            if (customer == null)
            {
                return HttpNotFound();
            }
            customer.IsActive = !customer.IsActive;
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? (customer.IsActive ? "Đã mở khóa tài khoản " : "Đã khóa tài khoản ") + customer.Username;
            return RedirectToAction("Details", new { id });
        }
    }
}
