using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    // Trang Home
    public class HomeProductVM
    {
        public string SearchTerm { get; set; }
        public List<Product> FeaturedProducts { get; set; } = new List<Product>(); // top 10 ban chay
        public IPagedList<Product> NewProducts { get; set; }                      // 20 san pham it nguoi mua, 6/trang
    }

    // Danh sach san pham theo danh muc
    public class ProductListVM
    {
        public Category Category { get; set; }
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public IPagedList<Product> Products { get; set; }
    }

    // Chi tiet san pham
    public class ProductDetailVM
    {
        public Product Product { get; set; }
        public int Quantity { get; set; } = 1;
        public List<Product> RelatedProducts { get; set; } = new List<Product>(); // 8 sp cung danh muc
        public IPagedList<Product> TopProducts { get; set; }                    // ban chay cung danh muc, 3/trang
    }

    // Menu ca nhan tren header
    public class PersonalMenuVM
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int CartCount { get; set; }
    }

    public class RegisterVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Chỉ dùng chữ không dấu, số, dấu _ và .")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Nhập lại mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(250)]
        public string CustomerAddress { get; set; }
    }

    public class UpdateProfileVM
    {
        public string Username { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(250)]
        public string CustomerAddress { get; set; }
    }

    public class ChangePasswordVM
    {
        [Display(Name = "Mật khẩu hiện tại")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Nhập lại mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; }
    }

    public class ProfileVM
    {
        public UpdateProfileVM Profile { get; set; }
        public ChangePasswordVM ChangePassword { get; set; } = new ChangePasswordVM();
    }

    // Trang thanh toan
    public class CheckoutVM
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Hình thức giao hàng")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        public string PaymentMethod { get; set; }

        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }

    // Trang Don hang cua toi
    public class MyOrderVM
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }   // null = Tat ca don
        public string Expand { get; set; }   // trang thai dang "Xem them"
        public bool HasAnyOrder { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
        public List<OrderGroup> Groups { get; set; } = new List<OrderGroup>();
    }

    public class OrderGroup
    {
        public string Status { get; set; }
        public int TotalCount { get; set; }
        public bool Expanded { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
