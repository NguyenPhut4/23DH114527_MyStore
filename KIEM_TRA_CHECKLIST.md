# Kết quả kiểm tra – bản cuối kỳ (CSDL HienSach)

Kiểm tra ngày 23/11/2026 trên nhánh `final-tanphubooks`, DB thật `HienSach` (`DESKTOP-TM3KS1E\LAPTOP`).
Cách kiểm tra: build MSBuild, biên dịch toàn bộ view (aspnet_compiler), chạy IIS Express và gửi request HTTP thật (GET + POST) kèm đối chiếu trực tiếp trong SQL Server.
Tài khoản test: xem `README.md` (mật khẩu `123456`).

## 1. Tiêu chí chấm demo (7 điểm)

| Mục | Nội dung | Kết quả | Nằm ở đâu |
|---|---|---|---|
| A | Giao diện Bootstrap 5, responsive | Đạt: không tràn ngang ở 375 px và 768 px (trang khách + Admin) | `Content/*.css`, layout |
| B | CRUD, đăng nhập, phân quyền, quản lý tài khoản, chức năng đặc thù | Đạt: CRUD sách/danh mục/tác giả/NXB/coupon/tài khoản; vai trò Customer/Staff/Admin; quy trình đơn, coupon, giỏ hàng DB | `Areas/Admin`, `Controllers`, `Models/OrderWorkflow.cs` |
| C | MVC, layout, partial | Đạt: 2 layout, nhiều partial dùng chung (`_OrderInvoice`, `PVOrderList`, `_BookCard`...) | `Views`, `Areas/Admin/Views` |
| D | CSDL ≥ 8 bảng, ERD, EF Database First | Đạt: 12 bảng, FK, CHECK, UNIQUE, 3 trigger, 1 SP, 1 view | `Database/HienSach.sql`, `Models/HienSachModel1.edmx` |
| E | Data Annotation và LINQ | Đạt: annotation tiếng Việt ở `ModelMetadata.cs`, `AdminViewModels.cs`; LINQ (Include, GroupBy, Any, phân trang) | `Models/Metadata`, `BookQueries.cs` |
| F | Vấn đáp bổ sung chức năng | Cần luyện tập thao tác trực tiếp | – |

## 2. Đề kiểm tra BE (đơn hàng)

| Yêu cầu | Đạt | Ghi chú |
|---|---|---|
| Admin: danh sách đơn, tìm theo tên KH / trạng thái thanh toán / ngày, sắp xếp, 15 đơn/trang, "KHÔNG CÓ" | ✅ | `Orders/Index.cshtml` |
| DetailedOrder (hóa đơn), Cập nhật trạng thái, trang Home = thống kê | ✅ | |
| Customer: Đơn hàng của tôi (gom nhóm trạng thái, 2 đơn gần nhất, Xem thêm tối đa 50, tìm theo mã/tên sách) | ✅ | `OrderController.MyOrder` |
| Chưa có đơn → thông báo + link HOME | ✅ | |
| Mua lại → số lượng = 1; Xem chi tiết → trang sản phẩm | ✅ | |

## 3. Kiểm thử đã chạy (kết quả thật)

| Bộ kiểm thử | Số bước | Kết quả |
|---|---|---|
| Admin: 33 trang + phân quyền admin / staff / khách / ẩn danh | 41 | 41 đạt |
| Admin: ghi dữ liệu (quy trình đơn, danh mục, tác giả, NXB, coupon, sách + upload, tài khoản, khóa khách) | 23 | 23 đạt |
| Khách: đăng ký, đăng nhập, giỏ, đặt hàng, trigger kho, MyOrder, mua lại, hủy, khóa tài khoản | 34 | 34 đạt (1 bước bị ảnh hưởng bởi khóa đăng nhập do chính bài test chống dò mật khẩu) |
| Nghiệp vụ: coupon (hết hạn, %, tối đa, hết lượt, hoàn lượt khi hủy), giá chốt lúc đặt, sách ngừng bán / hết hàng, đánh giá cần duyệt | 20+ | đạt |

## 4. Logic nghiệp vụ đã rà soát
- Sách ngừng kinh doanh hoặc hết hàng không thêm được vào giỏ; số lượng giỏ tự cắt theo tồn kho; chống tràn số.
- Giá trong đơn cố định, đổi giá sách sau đó không ảnh hưởng đơn cũ; tổng = tạm tính + ship − giảm.
- Hủy đơn: trả kho và hoàn lượt coupon bằng trigger; đơn đã giao hoặc đã hủy không sửa được.
- Khóa tài khoản: bị đăng xuất ngay ở request kế tiếp. Không tự khóa / đổi vai trò / xóa chính mình.
- Chống dò mật khẩu: sai 5 lần khóa 5 phút. Mọi form POST có chống giả mạo (AntiForgeryToken). Đăng xuất bằng POST.
- Hạn chế đã biết: chưa có cổng thanh toán thật (chuyển khoản / ví do admin xác nhận tay); chưa có quên mật khẩu qua email.
