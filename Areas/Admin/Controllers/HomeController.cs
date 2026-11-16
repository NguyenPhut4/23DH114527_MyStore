using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Trang Home Admin: thong ke
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            var activeOrders = db.Orders.Where(o => o.OrderStatus != OrderStatuses.Cancelled);

            var vm = new DashboardVM
            {
                TotalOrders = db.Orders.Count(),
                TotalRevenue = activeOrders.Select(o => (decimal?)o.TotalAmount).Sum() ?? 0,
                TotalCustomers = db.Customers.Count(),
                TotalProducts = db.Products.Count()
            };

            // Don hang theo trang thai (giu dung thu tu trang thai)
            var statusCounts = db.Orders.GroupBy(o => o.OrderStatus)
                                        .Select(g => new { Status = g.Key, Count = g.Count() })
                                        .ToList();
            vm.OrdersByStatus = OrderStatuses.All
                .Select(s => new ChartItem { Label = s, Value = statusCounts.Where(x => x.Status == s).Select(x => x.Count).FirstOrDefault() })
                .ToList();

            // Doanh thu 12 thang gan nhat
            var firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
            var monthly = activeOrders.Where(o => o.OrderDate >= firstMonth)
                                      .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                                      .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
                                      .ToList();
            for (var i = 0; i < 12; i++)
            {
                var m = firstMonth.AddMonths(i);
                vm.RevenueByMonth.Add(new ChartItem
                {
                    Label = m.ToString("MM/yyyy"),
                    Value = monthly.Where(x => x.Year == m.Year && x.Month == m.Month).Select(x => x.Total).FirstOrDefault()
                });
            }

            // Top 5 san pham ban chay
            vm.TopProducts = db.Products.OrderByDescending(p => p.SoldQuantity)
                                        .Take(5)
                                        .Select(p => new { p.ProductName, p.SoldQuantity })
                                        .ToList()
                                        .Select(p => new ChartItem { Label = p.ProductName, Value = p.SoldQuantity })
                                        .ToList();

            var products = db.Products.Include(p => p.Category).OrderBy(p => p.Quantity).ToList();
            vm.InStockProducts = products.Where(p => p.Quantity > 0).Take(5).ToList();
            vm.OutOfStockProducts = products.Where(p => p.Quantity <= 0).ToList();

            return View(vm);
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
