using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Controllers;
using NguyenTanPhu_23DH114527.Models;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    // Mã giảm giá: chỉ Admin quản lý
    [AdminAuthorize(Roles = "Admin")]
    public class CouponsController : BaseController
    {
        public ActionResult Index()
        {
            return View(db.Coupons.OrderByDescending(c => c.EndDate).ToList());
        }

        public ActionResult Create()
        {
            return View(new Coupon { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(1), UsageLimit = 100, DiscountPercent = 10, IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Coupon c)
        {
            c.Code = (c.Code ?? "").Trim().ToUpperInvariant();
            ModelState.Remove("Code");
            Validate(c);
            if (ModelState.IsValid)
            {
                c.UsedCount = 0;
                db.Coupons.Add(c);
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã thêm mã " + c.Code; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            return View(c);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Coupons.Find(id);
            return c == null ? (ActionResult)HttpNotFound() : View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Coupon input)
        {
            input.Code = (input.Code ?? "").Trim().ToUpperInvariant();
            ModelState.Remove("Code");
            Validate(input);
            if (ModelState.IsValid)
            {
                var c = db.Coupons.Find(input.CouponId);
                if (c == null) return HttpNotFound();
                c.Code = input.Code; c.DiscountPercent = input.DiscountPercent; c.MaxDiscount = input.MaxDiscount;
                c.MinOrderAmount = input.MinOrderAmount; c.StartDate = input.StartDate; c.EndDate = input.EndDate;
                c.UsageLimit = input.UsageLimit; c.IsActive = input.IsActive;
                var error = DbErrors.Save(db);
                if (error == null) { TempData["Success"] = "Đã cập nhật mã " + c.Code; return RedirectToAction("Index"); }
                ModelState.AddModelError("", error);
            }
            return View(input);
        }

        // Mã đã dùng trong đơn hàng thì không xóa, chỉ tắt áp dụng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var c = db.Coupons.Find(id);
            if (c == null) return HttpNotFound();
            if (db.Orders.Any(o => o.CouponId == id))
            {
                TempData["Error"] = "Mã " + c.Code + " đã được dùng trong đơn hàng nên không thể xóa. Hãy tắt \"Đang áp dụng\".";
                return RedirectToAction("Index");
            }
            db.Coupons.Remove(c);
            var error = DbErrors.Save(db);
            TempData[error == null ? "Success" : "Error"] = error ?? "Đã xóa mã " + c.Code;
            return RedirectToAction("Index");
        }

        private void Validate(Coupon c)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(c.Code, @"^[A-Z0-9]{3,20}$"))
                ModelState.AddModelError("Code", "Mã gồm 3-20 ký tự chữ hoa hoặc số, không dấu");
            else if (db.Coupons.Any(x => x.Code == c.Code && x.CouponId != c.CouponId))
                ModelState.AddModelError("Code", "Mã coupon đã tồn tại");
            if (c.EndDate < c.StartDate)
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải sau ngày bắt đầu");
        }
    }
}
