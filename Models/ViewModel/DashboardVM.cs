using System.Collections.Generic;

namespace NguyenTanPhu_23DH114527.Models.ViewModel
{
    public class DashboardVM
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }

        // Label -> gia tri, dung cho Google Chart
        public List<ChartItem> OrdersByStatus { get; set; } = new List<ChartItem>();
        public List<ChartItem> RevenueByMonth { get; set; } = new List<ChartItem>();
        public List<ChartItem> TopProducts { get; set; } = new List<ChartItem>();

        public List<Product> InStockProducts { get; set; } = new List<Product>();
        public List<Product> OutOfStockProducts { get; set; } = new List<Product>();
    }

    public class ChartItem
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }
}
