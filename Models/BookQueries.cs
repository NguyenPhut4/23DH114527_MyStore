using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NguyenTanPhu_23DH114527.Models
{
    // Các truy vấn LINQ dùng chung cho nhiều controller
    public static class BookQueries
    {
        // Số trang hợp lệ: chặn page = 0, âm hoặc rỗng (tránh lỗi 500 của PagedList)
        public static int Page(int? page)
        {
            return page.HasValue && page.Value > 0 ? page.Value : 1;
        }

        // Sách đang kinh doanh và còn hàng (trang khách hàng)
        public static IQueryable<Book> Available(HienSachEntities db)
        {
            return db.Books.Where(b => b.IsActive && b.StockQuantity > 0);
        }

        public static IQueryable<Book> WithAuthors(this IQueryable<Book> books)
        {
            return books.Include(b => b.Category).Include(b => b.BookAuthors.Select(a => a.Author));
        }

        // Tìm theo tên sách, tên tác giả, danh mục hoặc ISBN
        public static IQueryable<Book> Search(this IQueryable<Book> books, string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return books;
            }
            term = term.Trim();
            return books.Where(b => b.Title.Contains(term)
                                 || b.ISBN.Contains(term)
                                 || b.Category.CategoryName.Contains(term)
                                 || b.BookAuthors.Any(ba => ba.Author.AuthorName.Contains(term)));
        }

        // Sắp xếp theo số lượng đã bán (không tính đơn đã hủy)
        public static IQueryable<Book> OrderByBestSelling(this IQueryable<Book> books)
        {
            return books.OrderByDescending(b => b.OrderItems.Where(oi => oi.Order.Status != OrderStatuses.Cancelled).Sum(oi => (int?)oi.Quantity) ?? 0)
                        .ThenBy(b => b.Title);
        }

        public static Dictionary<int, int> SoldCounts(HienSachEntities db)
        {
            return db.OrderItems.Where(oi => oi.Order.Status != OrderStatuses.Cancelled)
                     .GroupBy(oi => oi.BookId)
                     .Select(g => new { BookId = g.Key, Total = g.Sum(x => x.Quantity) })
                     .ToDictionary(x => x.BookId, x => x.Total);
        }

        public static IEnumerable<Book> FillSold(HienSachEntities db, IEnumerable<Book> books)
        {
            var list = books.ToList();
            if (list.Count > 0)
            {
                var sold = SoldCounts(db);
                foreach (var b in list)
                {
                    int n;
                    b.SoldCount = sold.TryGetValue(b.BookId, out n) ? n : 0;
                }
            }
            return list;
        }
    }

    // Quy tắc mã giảm giá, giống điều kiện trong stored procedure sp_PlaceOrder
    public static class CouponRules
    {
        /// <returns>null nếu dùng được; discount là số tiền giảm. Ngược lại là lý do không dùng được.</returns>
        public static string Evaluate(HienSachEntities db, string code, decimal subTotal, out decimal discount)
        {
            discount = 0m;
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }
            code = code.Trim().ToUpperInvariant();
            var coupon = db.Coupons.FirstOrDefault(c => c.Code == code);
            var now = DateTime.Now;
            if (coupon == null || !coupon.IsActive)
            {
                return "Mã giảm giá không tồn tại hoặc đã ngừng áp dụng.";
            }
            if (now < coupon.StartDate)
            {
                return "Mã giảm giá chưa đến thời gian áp dụng.";
            }
            if (now > coupon.EndDate)
            {
                return "Mã giảm giá đã hết hạn.";
            }
            if (coupon.UsedCount >= coupon.UsageLimit)
            {
                return "Mã giảm giá đã hết lượt sử dụng.";
            }
            if (subTotal < coupon.MinOrderAmount)
            {
                return "Đơn hàng cần từ " + coupon.MinOrderAmount.ToString("N0") + " đ để dùng mã này.";
            }
            var value = Math.Round(subTotal * coupon.DiscountPercent / 100m, 0, MidpointRounding.AwayFromZero);
            discount = coupon.MaxDiscount.HasValue && value > coupon.MaxDiscount.Value ? coupon.MaxDiscount.Value : value;
            return null;
        }
    }
}
