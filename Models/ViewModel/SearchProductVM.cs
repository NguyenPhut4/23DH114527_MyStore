using System.ComponentModel.DataAnnotations;
using PagedList;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    public class SearchProductVM
    {
        [Display(Name = "Từ khóa")]
        public string SearchTerm { get; set; }

        [Display(Name = "Giá từ")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Giá đến")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Sắp xếp")]
        public string SortOrder { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 10;

        public IPagedList<Product> Products { get; set; }
    }
}
