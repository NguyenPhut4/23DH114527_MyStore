using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

namespace NguyenTanPhu_23DH114527.Models
{
    // Luu thay doi xuong DB; neu DB tu choi (CHECK / UNIQUE / FK) thi tra ve
    // thong bao tieng Viet thay vi de trang loi vang.
    public static class DbErrors
    {
        /// <returns>null neu luu thanh cong, nguoc lai la thong bao loi.</returns>
        public static string Save(DbContext db)
        {
            try
            {
                db.SaveChanges();
                return null;
            }
            catch (DbEntityValidationException ex)
            {
                var first = ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors).FirstOrDefault();
                return "Dữ liệu không hợp lệ: " + (first != null ? first.ErrorMessage : ex.Message);
            }
            catch (DbUpdateException ex)
            {
                var sql = ex.GetBaseException() as SqlException;
                if (sql != null)
                {
                    switch (sql.Number)
                    {
                        case 2627:
                        case 2601:
                            return "Dữ liệu bị trùng với bản ghi đã có (" + ConstraintName(sql.Message) + ").";
                        case 547:
                            return sql.Message.Contains("REFERENCE")
                                ? "Không thể thực hiện vì dữ liệu đang được sử dụng ở bảng khác (" + ConstraintName(sql.Message) + ")."
                                : "Dữ liệu vi phạm ràng buộc của hệ thống (" + ConstraintName(sql.Message) + ").";
                    }
                }
                return "Không thể lưu dữ liệu. Vui lòng thử lại.";
            }
        }

        // Lay ten rang buoc trong thong bao loi cua SQL Server,
        // vd: CHECK constraint "CK_Product_Price" / UNIQUE KEY constraint 'UQ_Customer_Phone'
        private static string ConstraintName(string message)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, "constraint [\"']([^\"']+)[\"']");
            return match.Success ? match.Groups[1].Value : "ràng buộc dữ liệu";
        }
    }
}
