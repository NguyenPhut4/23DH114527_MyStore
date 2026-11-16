using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Areas.Admin.Filters;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductsController : Controller
    {
        private const string ImageFolder = "~/Content/images/";
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products  (tim kiem + sap xep + phan trang)
        public ActionResult Index(SearchProductVM model, int? page)
        {
            var products = db.Products.Include(p => p.Category).AsQueryable();

            // Tim theo ten, mo ta hoac ten danh muc
            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                var term = model.SearchTerm.Trim();
                products = products.Where(p => p.ProductName.Contains(term)
                                            || p.ProductDescription.Contains(term)
                                            || p.Category.CategoryName.Contains(term));
            }
            if (model.MinPrice.HasValue)
            {
                var min = model.MinPrice.Value;
                products = products.Where(p => p.ProductPrice >= min);
            }
            if (model.MaxPrice.HasValue)
            {
                var max = model.MaxPrice.Value;
                products = products.Where(p => p.ProductPrice <= max);
            }

            switch (model.SortOrder)
            {
                case "name_asc": products = products.OrderBy(p => p.ProductName); break;
                case "name_desc": products = products.OrderByDescending(p => p.ProductName); break;
                case "price_asc": products = products.OrderBy(p => p.ProductPrice); break;
                case "price_desc": products = products.OrderByDescending(p => p.ProductPrice); break;
                case "qty_asc": products = products.OrderBy(p => p.Quantity); break;
                default: products = products.OrderByDescending(p => p.ProductID); break;
            }

            model.PageNumber = page ?? 1;
            model.Products = products.ToPagedList(model.PageNumber, model.PageSize);
            return View(model);
        }

        // GET: Admin/Products/Stock  (quan ly kho: hang ton / hang can nhap)
        public ActionResult Stock()
        {
            var products = db.Products.Include(p => p.Category).OrderBy(p => p.ProductName).ToList();
            ViewBag.InStock = products.Where(p => p.Quantity > 0).ToList();
            ViewBag.OutOfStock = products.Where(p => p.Quantity <= 0).ToList();
            return View();
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            return FindProduct(id, "Details");
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            LoadCategories(null);
            return View(new Product());
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,Quantity,UploadImg")] Product product)
        {
            ValidateImage(product);
            if (ModelState.IsValid)
            {
                SaveUploadedImage(product);
                product.SoldQuantity = 0;
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Đã thêm sản phẩm \"" + product.ProductName + "\"";
                return RedirectToAction("Index");
            }
            LoadCategories(product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            var result = FindProduct(id, "Edit");
            if (result is ViewResult view)
            {
                LoadCategories(((Product)view.Model).CategoryID);
            }
            return result;
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,Quantity,UploadImg")] Product product)
        {
            ValidateImage(product);
            if (ModelState.IsValid)
            {
                var existing = db.Products.Find(product.ProductID);
                if (existing == null)
                {
                    return HttpNotFound();
                }
                SaveUploadedImage(product);

                existing.CategoryID = product.CategoryID;
                existing.ProductName = product.ProductName;
                existing.ProductDescription = product.ProductDescription;
                existing.ProductPrice = product.ProductPrice;
                existing.ProductImage = product.ProductImage;
                existing.Quantity = product.Quantity;
                db.SaveChanges();

                TempData["Success"] = "Đã cập nhật sản phẩm";
                return RedirectToAction("Index");
            }
            LoadCategories(product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            return FindProduct(id, "Delete");
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Kiem tra rang buoc voi OrderDetail
            if (db.OrderDetails.Any(od => od.ProductID == id))
            {
                TempData["Error"] = "Không thể xóa sản phẩm \"" + product.ProductName + "\" vì đã có trong đơn hàng.";
                return RedirectToAction("Delete", new { id });
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa sản phẩm";
            return RedirectToAction("Index");
        }

        // Ham dung chung cho Details / Edit / Delete (GET)
        private ActionResult FindProduct(int? id, string viewName)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(viewName, product);
        }

        private void LoadCategories(int? selectedId)
        {
            ViewBag.CategoryID = new SelectList(db.Categories.OrderBy(c => c.CategoryName), "CategoryID", "CategoryName", selectedId);
        }

        private void ValidateImage(Product product)
        {
            if (product.UploadImg == null || product.UploadImg.ContentLength == 0)
            {
                return;
            }
            var ext = Path.GetExtension(product.UploadImg.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
            {
                ModelState.AddModelError("UploadImg", "Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .gif, .webp");
            }
            else if (product.UploadImg.ContentLength > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("UploadImg", "Ảnh tối đa 5MB");
            }
        }

        // Luu anh vao Content/images va gan duong dan cho ProductImage
        private void SaveUploadedImage(Product product)
        {
            if (product.UploadImg == null || product.UploadImg.ContentLength == 0)
            {
                return;
            }
            var folder = Server.MapPath(ImageFolder);
            Directory.CreateDirectory(folder);

            var name = Path.GetFileNameWithoutExtension(product.UploadImg.FileName);
            var ext = Path.GetExtension(product.UploadImg.FileName).ToLowerInvariant();
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + MakeSafeFileName(name) + ext;

            product.UploadImg.SaveAs(Path.Combine(folder, fileName));
            product.ProductImage = ImageFolder + fileName;
        }

        private static string MakeSafeFileName(string name)
        {
            var chars = name.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray();
            var safe = new string(chars);
            return string.IsNullOrEmpty(safe) ? "image" : (safe.Length > 50 ? safe.Substring(0, 50) : safe);
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
