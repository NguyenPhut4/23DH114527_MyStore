using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    // Mot mat hang trong gio
    [Serializable]
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }     // so luong ton tai thoi diem them vao gio
        public bool Selected { get; set; } = true; // tickbox chon mua

        public decimal TotalPrice => UnitPrice * Quantity;
    }

    // Quan ly danh sach mat hang trong gio
    [Serializable]
    public class Cart
    {
        private readonly List<CartItem> items = new List<CartItem>();

        public IReadOnlyList<CartItem> Items => items;

        // Nhom theo danh muc (gio hang nang cao)
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        // San pham cung danh muc voi cac san pham trong gio (khong luu vao session)
        [NonSerialized]
        private IPagedList<Product> similarProducts;
        public IPagedList<Product> SimilarProducts
        {
            get { return similarProducts; }
            set { similarProducts = value; }
        }

        public int TotalQuantity => items.Sum(i => i.Quantity);
        public IEnumerable<CartItem> SelectedItems => items.Where(i => i.Selected);
        public decimal SelectedTotal => SelectedItems.Sum(i => i.TotalPrice);
        public int SelectedQuantity => SelectedItems.Sum(i => i.Quantity);
        public bool IsEmpty => items.Count == 0;

        // Them san pham; tra ve so luong thuc te trong gio (khong vuot qua ton kho)
        public int AddItem(Product product, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == product.ProductID);
            if (item == null)
            {
                item = new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductImage = product.ImageUrl,
                    Category = product.Category != null ? product.Category.CategoryName : "Khác",
                    UnitPrice = product.ProductPrice,
                    Quantity = 0
                };
                items.Add(item);
            }
            item.MaxQuantity = product.Quantity;
            item.UnitPrice = product.ProductPrice;
            item.Selected = true;
            item.Quantity = Math.Min(item.Quantity + Math.Max(1, quantity), Math.Max(1, product.Quantity));
            return item.Quantity;
        }

        // MUA LAI (theo de Ktra): san pham co trong gio voi so luong = 1.
        // Da co san trong gio thi dat lai thanh 1; khong dong cham cac mon khac.
        public void SetItemQuantityOne(Product product)
        {
            var item = items.FirstOrDefault(i => i.ProductID == product.ProductID);
            if (item == null)
            {
                AddItem(product, 1);
                return;
            }
            item.MaxQuantity = product.Quantity;
            item.UnitPrice = product.ProductPrice;
            item.Quantity = 1;
            item.Selected = true;
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item == null)
            {
                return;
            }
            if (quantity <= 0)
            {
                RemoveItem(productId);
                return;
            }
            item.Quantity = item.MaxQuantity > 0 ? Math.Min(quantity, item.MaxQuantity) : quantity;
        }

        public void SetSelected(int productId, bool selected)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                item.Selected = selected;
            }
        }

        public void SelectAll(bool selected)
        {
            items.ForEach(i => i.Selected = selected);
        }

        public void RemoveSelected()
        {
            items.RemoveAll(i => i.Selected);
        }

        public void Clear()
        {
            items.Clear();
        }
    }

    // Tuong tac voi gio hang luu trong Session
    public class CartService
    {
        private const string SessionKey = "Cart";
        private readonly HttpSessionStateBase session;

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public Cart GetCart()
        {
            var cart = session[SessionKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session[SessionKey] = cart;
            }
            return cart;
        }

        public void ClearCart()
        {
            session[SessionKey] = null;
        }
    }
}
