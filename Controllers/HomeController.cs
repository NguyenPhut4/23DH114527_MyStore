using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NguyenTanPhu_23DH114527.Models;
using NguyenTanPhu_23DH114527.Models.ViewModel;
using PagedList;

namespace NguyenTanPhu_23DH114527.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // San pham con hang (ton > 0) co tim kiem theo ten / mo ta / danh muc
        private IQueryable<Product> AvailableProducts(string searchTerm)
        {
            var products = db.Products.Include(p => p.Category).Where(p => p.Quantity > 0);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                products = products.Where(p => p.ProductName.Contains(term)
                                            || p.ProductDescription.Contains(term)
                                            || p.Category.CategoryName.Contains(term));
            }
            return products;
        }

        // GET: Home/Index?searchTerm=&page=
        public ActionResult Index(string searchTerm, int? page)
        {
            var products = AvailableProducts(searchTerm);

            var vm = new HomeProductVM
            {
                SearchTerm = searchTerm,
                // Top 10 san pham ban chay nhat
                FeaturedProducts = products.OrderByDescending(p => p.SoldQuantity).ThenBy(p => p.ProductName).Take(10).ToList(),
                // 20 san pham it nguoi mua nhat, phan trang 6 sp/trang
                NewProducts = products.OrderBy(p => p.SoldQuantity).ThenByDescending(p => p.ProductID)
                                      .Take(20).ToList()
                                      .ToPagedList(page ?? 1, 6)
            };
            return View(vm);
        }

        // GET: Home/ProductList/5  (san pham theo danh muc)
        public ActionResult ProductList(int? id, string searchTerm, string sortOrder, decimal? minPrice, decimal? maxPrice, int? page)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var products = AvailableProducts(searchTerm).Where(p => p.CategoryID == id);
            // Loc theo khoang gia
            if (minPrice.HasValue)
            {
                var min = minPrice.Value;
                products = products.Where(p => p.ProductPrice >= min);
            }
            if (maxPrice.HasValue)
            {
                var max = maxPrice.Value;
                products = products.Where(p => p.ProductPrice <= max);
            }
            switch (sortOrder)
            {
                case "name_asc": products = products.OrderBy(p => p.ProductName); break;
                case "name_desc": products = products.OrderByDescending(p => p.ProductName); break;
                case "price_asc": products = products.OrderBy(p => p.ProductPrice); break;
                case "price_desc": products = products.OrderByDescending(p => p.ProductPrice); break;
                default: products = products.OrderByDescending(p => p.SoldQuantity).ThenBy(p => p.ProductName); break;
            }

            var vm = new ProductListVM
            {
                Category = category,
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Products = products.ToPagedList(page ?? 1, 9)
            };
            return View(vm);
        }

        // GET: Home/ProductDetail/5?quantity=1&page=1
        public ActionResult ProductDetail(int? id, int? quantity, int? page)
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

            var sameCategory = db.Products.Where(p => p.CategoryID == product.CategoryID
                                                   && p.ProductID != product.ProductID
                                                   && p.Quantity > 0);
            var vm = new ProductDetailVM
            {
                Product = product,
                Quantity = quantity.HasValue && quantity.Value > 0 ? quantity.Value : 1,
                RelatedProducts = sameCategory.OrderByDescending(p => p.ProductID).Take(8).ToList(),
                TopProducts = sameCategory.OrderByDescending(p => p.SoldQuantity).Take(8).ToList()
                                          .ToPagedList(page ?? 1, 3)
            };
            return View(vm);
        }

        // Menu danh muc tren header
        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            return PartialView("_CategoriesPV", categories);
        }

        // Menu ca nhan tren header: Trang chu, Tai khoan, Gio hang
        [ChildActionOnly]
        public ActionResult PersonalMenu()
        {
            var vm = new PersonalMenuVM
            {
                IsLoggedIn = User.Identity.IsAuthenticated,
                Username = User.Identity.Name,
                DisplayName = Session["CustomerName"] as string ?? User.Identity.Name,
                CartCount = new CartService(Session).GetCart().TotalQuantity
            };
            return PartialView("_PersonalMenuPV", vm);
        }

        public ActionResult About()
        {
            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            return RedirectToAction("Index");
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
