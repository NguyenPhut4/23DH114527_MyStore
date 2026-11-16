using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

// Data Annotation dat o day (partial class + MetadataType)
// de khong bi mat khi "Update Model from Database".
namespace NguyenTanPhu_23DH114527.Models
{
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category { }

    public class CategoryMetadata
    {
        [Display(Name = "Mã danh mục")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        public string CategoryName { get; set; }
    }

    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
        public const string DefaultImage = "~/Content/images/no-image.svg";

        [NotMapped]
        [Display(Name = "Tải ảnh lên")]
        public HttpPostedFileBase UploadImg { get; set; }

        [NotMapped]
        public string ImageUrl
        {
            get { return string.IsNullOrWhiteSpace(ProductImage) ? DefaultImage : ProductImage; }
        }
    }

    public class ProductMetadata
    {
        [Display(Name = "Mã sản phẩm")]
        public int ProductID { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ 2 đến 200 ký tự")]
        public string ProductName { get; set; }

        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [DataType(DataType.MultilineText)]
        public string ProductDescription { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, 100000000, ErrorMessage = "Giá từ 1.000 đến 100.000.000")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ProductImage { get; set; }

        [Display(Name = "Số lượng tồn")]
        [Range(0, 100000, ErrorMessage = "Số lượng tồn từ 0 đến 100.000")]
        public int Quantity { get; set; }

        [Display(Name = "Đã bán")]
        public int SoldQuantity { get; set; }
    }

    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer { }

    public class CustomerMetadata
    {
        [Display(Name = "Mã KH")]
        public int CustomerID { get; set; }

        [Display(Name = "Tên khách hàng")]
        [Required]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required]
        [Phone]
        public string CustomerPhone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }

        [Display(Name = "Tài khoản")]
        public string Username { get; set; }
    }

    [MetadataType(typeof(OrderMetadata))]
    public partial class Order { }

    public class OrderMetadata
    {
        [Display(Name = "Mã đơn")]
        public int OrderID { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng giá trị")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Trạng thái đơn")]
        public string OrderStatus { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Phương thức giao hàng")]
        public string DeliveryMethod { get; set; }
    }

    [MetadataType(typeof(UserMetadata))]
    public partial class User { }

    public class UserMetadata
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Chỉ dùng chữ, số, dấu _ và .")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Vai trò")]
        public string UserRole { get; set; }
    }
}
