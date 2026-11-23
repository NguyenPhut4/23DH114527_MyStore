using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    public class ChartItem
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class DashboardVM
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal Revenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBooks { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public List<ChartItem> OrdersByStatus { get; set; } = new List<ChartItem>();
        public List<ChartItem> RevenueByMonth { get; set; } = new List<ChartItem>();
        public List<ChartItem> RevenueByCategory { get; set; } = new List<ChartItem>();   // theo danh mục gốc trong khoảng thời gian
        public List<ChartItem> TopAuthors { get; set; } = new List<ChartItem>();          // top 5 tác giả bán chạy
        public List<Book> LowStock { get; set; } = new List<Book>();
    }

    public class OrderSearchVM
    {
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        [DataType(DataType.Date)]
        public DateTime? OrderDate { get; set; }

        public string SortOrder { get; set; }
        public int PageSize { get; set; } = 15;
        public IPagedList<Order> Orders { get; set; }
    }

    public class BookSearchVM
    {
        [Display(Name = "Từ khóa")]
        public string SearchTerm { get; set; }

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Giá từ")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Giá đến")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Tình trạng")]
        public string StockFilter { get; set; }   // out, inactive

        public string SortOrder { get; set; }
        public int PageSize { get; set; } = 10;
        public IPagedList<Book> Books { get; set; }
    }

    public class UserSearchVM
    {
        [Display(Name = "Từ khóa")]
        public string SearchTerm { get; set; }

        public string SortOrder { get; set; }
        public int PageSize { get; set; } = 10;
        public IPagedList<User> Users { get; set; }
    }

    // Form tạo / sửa tài khoản quản trị và nhân viên
    public class UserFormVM
    {
        public int UserId { get; set; }

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Chỉ dùng chữ không dấu, số, dấu _ và .")]
        public string Username { get; set; }

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
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string Address { get; set; }

        [Display(Name = "Vai trò")]
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Mật khẩu")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
