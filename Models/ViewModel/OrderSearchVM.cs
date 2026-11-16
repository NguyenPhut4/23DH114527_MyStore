using System;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
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
}
