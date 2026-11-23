using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

// Phần mở rộng cho các lớp entity do EF tạo ra (partial class) và Data Annotation.
// Đặt ở đây để không bị mất khi chạy lại "Update Model from Database".
namespace NguyenTanPhu_23DH114527.Models
{
    [MetadataType(typeof(BookMetadata))]
    public partial class Book
    {
        public const string DefaultImage = "~/Content/images/no-image.svg";

        public HttpPostedFileBase UploadImg { get; set; }   // ảnh bìa tải lên (không lưu trong CSDL)
        public int SoldCount { get; set; }                  // số lượng đã bán, tính từ OrderItems

        public string ImageUrl
        {
            get { return string.IsNullOrWhiteSpace(CoverImage) ? DefaultImage : CoverImage; }
        }

        public string AuthorNames
        {
            get
            {
                return string.Join(", ", BookAuthors.Where(x => x.AuthorRole != "Dịch giả")
                                                    .OrderBy(x => x.AuthorRole == "Tác giả" ? 0 : 1)
                                                    .Select(x => x.Author.AuthorName));
            }
        }

        public string TranslatorNames
        {
            get { return string.Join(", ", BookAuthors.Where(x => x.AuthorRole == "Dịch giả").Select(x => x.Author.AuthorName)); }
        }
    }

    public class BookMetadata
    {
        [Display(Name = "Mã sách")]
        public int BookId { get; set; }

        [Display(Name = "Mã ISBN")]
        [Required(ErrorMessage = "Vui lòng nhập mã ISBN")]
        [RegularExpression(@"^(\d{9}[\dX]|\d{13})$", ErrorMessage = "ISBN gồm 10 hoặc 13 ký tự (chữ số, ký tự cuối có thể là X)")]
        public string ISBN { get; set; }

        [Display(Name = "Tên sách")]
        [Required(ErrorMessage = "Vui lòng nhập tên sách")]
        [StringLength(250, MinimumLength = 2, ErrorMessage = "Tên sách từ 2 đến 250 ký tự")]
        public string Title { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Nhà xuất bản")]
        [Required(ErrorMessage = "Vui lòng chọn nhà xuất bản")]
        public int PublisherId { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, 100000000, ErrorMessage = "Giá từ 1.000 đến 100.000.000")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "Số lượng tồn")]
        [Range(0, 100000, ErrorMessage = "Số lượng tồn từ 0 đến 100.000")]
        public int StockQuantity { get; set; }

        [Display(Name = "Số trang")]
        [Range(1, 10000, ErrorMessage = "Số trang từ 1 đến 10.000")]
        public Nullable<int> PageCount { get; set; }

        [Display(Name = "Năm xuất bản")]
        [Range(1900, 2100, ErrorMessage = "Năm xuất bản từ 1900 đến 2100")]
        public Nullable<int> PublishYear { get; set; }

        [Display(Name = "Ảnh bìa")]
        public string CoverImage { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Đang kinh doanh")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày thêm")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedAt { get; set; }
    }

    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Display(Name = "Mã danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        public string CategoryName { get; set; }

        [Display(Name = "Danh mục cha")]
        public Nullable<int> ParentId { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string Description { get; set; }
    }

    [MetadataType(typeof(AuthorMetadata))]
    public partial class Author
    {
    }

    public class AuthorMetadata
    {
        [Display(Name = "Mã tác giả")]
        public int AuthorId { get; set; }

        [Display(Name = "Tên tác giả")]
        [Required(ErrorMessage = "Vui lòng nhập tên tác giả")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên tác giả từ 2 đến 100 ký tự")]
        public string AuthorName { get; set; }

        [Display(Name = "Năm sinh")]
        [Range(1000, 2100, ErrorMessage = "Năm sinh từ 1000 đến 2100")]
        public Nullable<int> BirthYear { get; set; }

        [Display(Name = "Quốc tịch")]
        [StringLength(50, ErrorMessage = "Quốc tịch tối đa 50 ký tự")]
        public string Nationality { get; set; }

        [Display(Name = "Tiểu sử")]
        [DataType(DataType.MultilineText)]
        public string Biography { get; set; }
    }

    [MetadataType(typeof(PublisherMetadata))]
    public partial class Publisher
    {
    }

    public class PublisherMetadata
    {
        [Display(Name = "Mã NXB")]
        public int PublisherId { get; set; }

        [Display(Name = "Tên nhà xuất bản")]
        [Required(ErrorMessage = "Vui lòng nhập tên nhà xuất bản")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên NXB từ 2 đến 150 ký tự")]
        public string PublisherName { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string Address { get; set; }

        [Display(Name = "Điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10-11 chữ số, bắt đầu bằng 0")]
        public string Phone { get; set; }
    }

    [MetadataType(typeof(CouponMetadata))]
    public partial class Coupon
    {
        public bool IsUsable
        {
            get
            {
                var now = DateTime.Now;
                return IsActive && StartDate <= now && now <= EndDate && UsedCount < UsageLimit;
            }
        }
    }

    public class CouponMetadata
    {
        [Display(Name = "Mã coupon")]
        [Required(ErrorMessage = "Vui lòng nhập mã coupon")]
        [RegularExpression(@"^[A-Z0-9]{3,20}$", ErrorMessage = "Mã gồm 3-20 ký tự chữ hoa hoặc số, không dấu")]
        public string Code { get; set; }

        [Display(Name = "Phần trăm giảm")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm từ 1 đến 100")]
        public int DiscountPercent { get; set; }

        [Display(Name = "Giảm tối đa")]
        [Range(0, 100000000, ErrorMessage = "Giảm tối đa không hợp lệ")]
        public Nullable<decimal> MaxDiscount { get; set; }

        [Display(Name = "Đơn tối thiểu")]
        [Range(0, 100000000, ErrorMessage = "Đơn tối thiểu không hợp lệ")]
        public decimal MinOrderAmount { get; set; }

        [Display(Name = "Bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Giới hạn lượt dùng")]
        [Range(1, 1000000, ErrorMessage = "Giới hạn lượt dùng từ 1 đến 1.000.000")]
        public int UsageLimit { get; set; }

        [Display(Name = "Đã dùng")]
        public int UsedCount { get; set; }

        [Display(Name = "Đang áp dụng")]
        public bool IsActive { get; set; }
    }

    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
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
        public string Role { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedAt { get; set; }
    }

    [MetadataType(typeof(ReviewMetadata))]
    public partial class Review
    {
    }

    public class ReviewMetadata
    {
        [Display(Name = "Điểm đánh giá")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Display(Name = "Nhận xét")]
        [StringLength(1000, ErrorMessage = "Nhận xét tối đa 1000 ký tự")]
        public string Comment { get; set; }
    }

    [MetadataType(typeof(OrderMetadata))]
    public partial class Order
    {
        public string StatusText { get { return OrderStatuses.Text(Status); } }
        public string PaymentMethodText { get { return PayMethods.Text(PaymentMethod); } }
        public string PaymentStatusText { get { return PayStatuses.Text(PaymentStatus); } }
        public string DeliveryText { get { return DeliveryOptions.Text(DeliveryMethod); } }
    }

    public class OrderMetadata
    {
        [Display(Name = "Mã đơn")]
        public int OrderId { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Người nhận")]
        public string ReceiverName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string ReceiverPhone { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Phí vận chuyển")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal ShippingFee { get; set; }

        [Display(Name = "Giảm giá")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Tổng giá trị")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Trạng thái đơn")]
        public string Status { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Phương thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
    }
}
