# Hiên Sách – Website bán sách trực tuyến (ASP.NET MVC 5 + Entity Framework 6)

Đồ án cuối kỳ môn Lập trình Web – nhóm 17.
Sinh viên: **Nguyễn Tấn Phú – 23DH114527**

Website gồm 2 phần dùng chung một trang đăng nhập (`/Account/Login`), tự chuyển theo vai trò:
- **Customer**: trang chủ (top 10 bán chạy, sách mới), danh mục 2 cấp, lọc theo giá và sắp xếp, chi tiết sách (tác giả, dịch giả, NXB, đánh giá), giỏ hàng lưu trong CSDL, thanh toán (COD / chuyển khoản / ví điện tử, giao thường / nhanh, mã giảm giá), **Đơn hàng của tôi** (mua lại, hủy đơn chờ thanh toán, in hóa đơn), đánh giá sách đã nhận, hồ sơ cá nhân.
- **Admin / Staff** (`/Admin`): thống kê (doanh thu theo tháng và theo danh mục, top 5 tác giả), đơn hàng (tìm, sắp xếp, 15 đơn/trang, cập nhật trạng thái theo quy trình), sách (upload ảnh, tác giả/dịch giả), kho, danh mục, tác giả, NXB, đánh giá (duyệt/ẩn/xóa), khách hàng. Riêng **Admin**: coupon, quản lý tài khoản, khóa/mở khóa khách.

## Công nghệ
- ASP.NET MVC 5, .NET Framework 4.7.2, Razor, Areas
- Entity Framework 6 – Database First (`Models/HienSachModel1.edmx`)
- SQL Server (12 bảng, 3 trigger, 1 stored procedure, 1 view), Bootstrap 5, PagedList.Mvc, Google Charts
- Forms Authentication, phân quyền theo vai trò (`[AdminAuthorize]`), BCrypt băm mật khẩu

## Cài đặt và chạy
1. **Tạo database**: mở `Database/HienSach.sql` trong SSMS rồi Execute (F5). Script tự xóa DB `HienSach` cũ và tạo lại toàn bộ cùng dữ liệu mẫu. Hoặc dùng sqlcmd:
   ```
   sqlcmd -S "TEN_MAY\SQLEXPRESS" -E -C -b -f 65001 -i Database\HienSach.sql
   ```
2. **Chuỗi kết nối**: trong `Web.config`, mục `connectionStrings` → `HienSachEntities`, đổi `data source=...` thành SQL Server của máy bạn.
3. Mở `NguyenTanPhu_23DH114527.sln` bằng Visual Studio 2022, Build (tự tải NuGet) rồi nhấn **F5**.

## Tài khoản dùng thử (mật khẩu chung: `123456`)
| Vai trò | Tên đăng nhập |
|---|---|
| Admin | `admin` |
| Nhân viên (Staff) | `nhanvien01` |
| Khách hàng | `khoale`, `thuha.pham`, `namvo`, `lan.dang`, `huybui`, `truchuynh` |

Staff không vào được Coupon và Tài khoản (trang 403). Đăng nhập sai 5 lần thì bị khóa 5 phút.

## Quy tắc nghiệp vụ chính
- **Trạng thái đơn**: Chờ thanh toán → Đang xử lý → Đang vận chuyển → Đã giao; hoặc Đã hủy. Đã giao và Đã hủy là trạng thái cuối.
- **Thanh toán trước** (chuyển khoản, ví): admin phải tick "đã nhận tiền" mới chuyển sang Đang xử lý; không hủy được khi đã vận chuyển. **COD**: tự thành Đã thanh toán khi giao xong.
- **Khách chỉ hủy được đơn Chờ thanh toán.** Hủy đơn: trigger CSDL trả hàng về kho và hoàn lượt dùng coupon.
- **Phí ship**: giao thường 30.000 đ (miễn phí từ 300.000 đ), giao nhanh 45.000 đ. Giá trong đơn là giá tại thời điểm đặt.
- **Coupon**: kiểm tra hạn, lượt dùng, đơn tối thiểu, giảm tối đa. **Đặt hàng** chạy bằng `sp_PlaceOrder` (một transaction), tồn kho trừ bằng trigger.
- **Đánh giá**: chỉ khách đã nhận sách, mỗi sách một đánh giá (sửa được), cần admin duyệt mới hiển thị.

## Sao lưu và khôi phục
| Tình huống | Cách xử lý |
|---|---|
| Sao lưu trước khi demo | `Database/backup_hiensach.ps1` |
| Xóa nhầm dữ liệu hoặc mất DB | `Database/restore_hiensach.ps1` (lấy bản backup mới nhất) |
| Máy mới | Chạy lại `Database/HienSach.sql` |
| Lỗi "wait operation timed out" | Bấm F5; nếu lặp lại thì tắt bớt ứng dụng nặng (máy thiếu RAM) |
