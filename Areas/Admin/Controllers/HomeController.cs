using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Trang Home của Admin: thống kê
    [AdminAuthorize]
    public class HomeController : BaseController
    {
        // GET: Admin/Home?from=2026-01-01&to=2026-11-30
        public ActionResult Index(DateTime? from, DateTime? to)
        {
            var today = DateTime.Today;
            var start = from ?? new DateTime(today.Year, today.Month, 1).AddMonths(-11);
            var end = (to ?? today).Date;
            if (end < start)
            {
                end = start;
            }

            var active = db.Orders.Where(o => o.Status != OrderStatuses.Cancelled);
            var vm = new DashboardVM
            {
                From = start,
                To = end,
                TotalOrders = db.Orders.Count(),
                PendingOrders = db.Orders.Count(o => o.Status == OrderStatuses.Pending),
                Revenue = active.Select(o => (decimal?)o.TotalAmount).Sum() ?? 0,
                TotalCustomers = db.Users.Count(u => u.Role == UserRoles.Customer),
                TotalBooks = db.Books.Count()
            };

            // Đơn hàng theo trạng thái (giữ đúng thứ tự trạng thái)
            var statusCounts = db.Orders.GroupBy(o => o.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
            vm.OrdersByStatus = OrderStatuses.All.Select(s => new ChartItem
            {
                Label = OrderStatuses.Text(s),
                Value = statusCounts.Where(x => x.Status == s).Select(x => x.Count).FirstOrDefault()
            }).ToList();

            // Doanh thu 12 tháng gần nhất
            var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
            var monthly = active.Where(o => o.OrderDate >= firstMonth)
                                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) }).ToList();
            for (var i = 0; i < 12; i++)
            {
                var m = firstMonth.AddMonths(i);
                vm.RevenueByMonth.Add(new ChartItem
                {
                    Label = m.ToString("MM/yyyy"),
                    Value = monthly.Where(x => x.Year == m.Year && x.Month == m.Month).Select(x => x.Total).FirstOrDefault()
                });
            }

            // Doanh thu theo danh mục gốc trong khoảng thời gian
            var endExclusive = end.AddDays(1);
            vm.RevenueByCategory = db.OrderItems
                .Where(oi => oi.Order.Status != OrderStatuses.Cancelled && oi.Order.OrderDate >= start && oi.Order.OrderDate < endExclusive)
                .GroupBy(oi => oi.Book.Category.ParentCategory != null ? oi.Book.Category.ParentCategory.CategoryName : oi.Book.Category.CategoryName)
                .Select(g => new { Label = g.Key, Value = g.Sum(oi => oi.Quantity * oi.UnitPrice) })
                .OrderByDescending(x => x.Value).ToList()
                .Select(x => new ChartItem { Label = x.Label, Value = x.Value }).ToList();

            // Top 5 tác giả có số lượng sách bán ra nhiều nhất (không tính dịch giả và đơn đã hủy)
            vm.TopAuthors = (from oi in db.OrderItems
                             where oi.Order.Status != OrderStatuses.Cancelled
                             from ba in oi.Book.BookAuthors
                             where ba.AuthorRole != "Dịch giả"
                             group oi by ba.Author.AuthorName into g
                             orderby g.Sum(x => x.Quantity) descending
                             select new { Name = g.Key, Sold = g.Sum(x => x.Quantity) }).Take(5).ToList()
                            .Select(x => new ChartItem { Label = x.Name, Value = x.Sold }).ToList();

            vm.LowStock = db.Books.Where(b => b.IsActive && b.StockQuantity <= 10).OrderBy(b => b.StockQuantity).ThenBy(b => b.Title).Take(8).ToList();
            return View(vm);
        }
    }
}
