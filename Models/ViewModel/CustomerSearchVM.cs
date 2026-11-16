using System.ComponentModel.DataAnnotations;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    public class CustomerSearchVM
    {
        [Display(Name = "Từ khóa")]
        public string SearchTerm { get; set; }

        public string SortOrder { get; set; }

        public int PageSize { get; set; } = 10;

        public IPagedList<Customer> Customers { get; set; }
    }

    public class UserSearchVM
    {
        [Display(Name = "Tên đăng nhập")]
        public string SearchTerm { get; set; }

        public int PageSize { get; set; } = 10;

        public IPagedList<User> Users { get; set; }
    }
}
