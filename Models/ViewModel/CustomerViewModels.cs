using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    // ===== Trang chủ, danh mục, chi tiết sách =====
    public class HomeBookVM
    {
        public string SearchTerm { get; set; }
        public List<Book> FeaturedBooks { get; set; } = new List<Book>();   // top 10 bán chạy
        public IPagedList<Book> NewBooks { get; set; }                       // sách mới, 8 cuốn/trang
    }

    public class BookListVM
    {
        public Category Category { get; set; }
        public List<Category> Children { get; set; } = new List<Category>();
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public IPagedList<Book> Books { get; set; }
    }

    public class BookDetailVM
    {
        public Book Book { get; set; }
        public int Quantity { get; set; } = 1;
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();
        public bool CanReview { get; set; }          // đã mua và nhận sách này
        public Review MyReview { get; set; }
        public List<Book> RelatedBooks { get; set; } = new List<Book>();
        public IPagedList<Book> TopBooks { get; set; }
    }

    public class PersonalMenuVM
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public int CartCount { get; set; }
    }

    // ===== Tài khoản =====
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
        public string FullName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string Address { get; set; }
    }

    public class LoginVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class UpdateProfileVM
    {
        public string Username { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(255)]
        public string Address { get; set; }
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

    // ===== Giỏ hàng (lưu trong bảng CartItems) =====
    public class CartLine
    {
        public Book Book { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get { return Book.Price * Quantity; } }
        public bool Available { get { return Book.IsActive && Book.StockQuantity >= Quantity; } }
    }

    public class CartVM
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();
        public IPagedList<Book> SimilarBooks { get; set; }

        public bool IsEmpty { get { return Lines.Count == 0; } }
        public int TotalQuantity { get { return Lines.Sum(l => l.Quantity); } }
        public decimal SubTotal { get { return Lines.Sum(l => l.LineTotal); } }
        public bool AllAvailable { get { return Lines.All(l => l.Available); } }
        // Nhóm theo danh mục sách
        public List<IGrouping<string, CartLine>> Grouped { get { return Lines.GroupBy(l => l.Book.Category.CategoryName).ToList(); } }
    }

    // ===== Thanh toán =====
    public class CheckoutVM
    {
        [Display(Name = "Người nhận")]
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự")]
        public string ReceiverName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string ReceiverPhone { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Hình thức giao hàng")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Mã giảm giá")]
        [StringLength(20, ErrorMessage = "Mã giảm giá tối đa 20 ký tự")]
        public string CouponCode { get; set; }

        public List<CartLine> Lines { get; set; } = new List<CartLine>();
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get { return SubTotal + ShippingFee - Discount; } }
        public string CouponMessage { get; set; }
        public string SelectedIds { get; set; }   // các BookId được chọn trong giỏ (vd "1,3"); rỗng = cả giỏ
    }

    // ===== Đơn hàng của tôi =====
    public class MyOrderVM
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }   // null = Tất cả đơn
        public string Expand { get; set; }   // trạng thái đang bấm "Xem thêm"
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
