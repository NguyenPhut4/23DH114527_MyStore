# Hiên Sách – Website bán sách (ASP.NET MVC5 + Entity Framework 6)

Đồ án môn Lập trình Web – phần Back-end (BE).
Sinh viên: **Nguyễn Tấn Phú – 23DH114527**

Website bán sách gồm 2 phần:
- **Customer**: trang chủ, sản phẩm theo danh mục, chi tiết sản phẩm, giỏ hàng, thanh toán, đăng ký / đăng nhập, **Đơn hàng của tôi**.
- **Admin** (`/Admin`): thống kê, quản lý danh mục, sản phẩm (upload ảnh, tìm kiếm, sắp xếp, phân trang), kho hàng, khách hàng, **quản lý đơn hàng**, tài khoản quản trị.

## Công nghệ
- ASP.NET MVC 5, .NET Framework 4.7.2
- Entity Framework 6 – Database First (`Models/MyStoreModel1.edmx`)
- SQL Server, Bootstrap 5, PagedList.Mvc, Google Charts

## Cài đặt và chạy

1. **Tạo database**: trong SQL Server tạo database tên `MyStore`, rồi chạy lần lượt:
   1. `Database/database2024.sql`: tạo các bảng.
   2. `Database/update_mystore.sql`: bổ sung cột (Quantity, OrderStatus, ...), dữ liệu mẫu và ảnh bìa sách.

   Chạy bằng SSMS, hoặc dùng sqlcmd:
   ```
   sqlcmd -S "TEN_MAY\SQLEXPRESS" -E -C -I -d MyStore -f 65001 -i Database\database2024.sql
   sqlcmd -S "TEN_MAY\SQLEXPRESS" -E -C -I -d MyStore -f 65001 -i Database\update_mystore.sql
   ```
2. **Sửa chuỗi kết nối**: trong `Web.config`, ở mục `connectionStrings` → `MyStoreEntities`, đổi `data source=...` thành tên SQL Server trên máy bạn.
3. Mở `NguyenTanPhu_23DH114527.sln` bằng Visual Studio 2022. Khi Build, Visual Studio tự tải các gói NuGet. Nhấn **F5** để chạy.

## Tài khoản dùng thử
| Vai trò | Tên đăng nhập | Mật khẩu | Đường dẫn |
|---|---|---|---|
| Admin | `admin` | `123456` | `/Admin` |
| Khách hàng | `khach1` (`khach2`, `khach3`) | `123456` | `/Account/Login` |

## Bài kiểm tra BE

| Yêu cầu | Vị trí |
|---|---|
| Admin – Danh sách đơn hàng: tìm theo Tên KH / Trạng thái thanh toán / Ngày đặt, sắp xếp, 15 đơn/trang, "KHÔNG CÓ" | `Areas/Admin/Controllers/OrdersController.cs`, `Areas/Admin/Views/Orders/Index.cshtml` |
| Admin – Chi tiết đơn hàng + Cập nhật trạng thái | `Areas/Admin/Views/Orders/DetailedOrder.cshtml`, `UpdateStatus.cshtml` |
| Admin – Trang Home thống kê | `Areas/Admin/Views/Home/Index.cshtml` |
| Customer – Đơn hàng của tôi: tab trạng thái, 2 đơn gần nhất / Xem thêm 50 đơn, tìm theo mã đơn hoặc tên sản phẩm, Mua lại, Xem chi tiết | `Controllers/OrderController.cs`, `Views/Order/MyOrder.cshtml` |

Kết quả đối chiếu Checklist BE: xem [KIEM_TRA_CHECKLIST.md](KIEM_TRA_CHECKLIST.md).

## Quy tắc trạng thái đơn hàng
Trạng thái thanh toán **không chọn tay**: hệ thống tự cập nhật theo trạng thái đơn (`Models/OrderWorkflow.cs`), và DB có ràng buộc `CK_Order_StatusPayment` chặn các tổ hợp vô lý.

| Hình thức | Luồng hợp lệ |
|---|---|
| **Chuyển khoản** | Chờ thanh toán → *(Admin tick "Đã nhận tiền")* → Đang xử lý → Đang vận chuyển → Đã giao. Chờ thanh toán / Đang xử lý có thể → Đã hủy. Đang vận chuyển chỉ còn → Đã giao |
| **Tiền mặt (COD)** | Đặt hàng là Đang xử lý → Đang vận chuyển → Đã giao (tự thành "Đã thanh toán"). Đang xử lý / Đang vận chuyển có thể → Đã hủy |

- "Đã giao" và "Đã hủy" là trạng thái cuối, không đổi được nữa. Hủy đơn sẽ trả hàng về kho.
- Đơn chuyển khoản đã thu tiền mà bị hủy thì hệ thống nhắc cần hoàn tiền cho khách.

## Ràng buộc dữ liệu
- **Khóa ngoại** (`ON DELETE NO ACTION`): không xóa được bản ghi đang được dùng ở bảng khác. Ví dụ: danh mục còn sản phẩm, sản phẩm đã có trong đơn hàng.
- **CHECK**: giá > 0, tồn kho ≥ 0, số lượng mua > 0, trạng thái đơn / trạng thái thanh toán / vai trò phải đúng danh sách cho phép.
- **UNIQUE**: mỗi tài khoản chỉ gắn 1 khách hàng, số điện thoại không trùng.
- Code kiểm tra thêm bằng Data Annotation. Nếu DB từ chối dữ liệu, web hiện thông báo tiếng Việt (`Models/DbErrors.cs`) thay vì trang lỗi.

## Sao lưu và khôi phục
| Tình huống | Cách làm |
|---|---|
| Sao lưu dữ liệu hiện tại | Chạy `Database/backup_mystore.ps1`. File `.bak` lưu trong thư mục Backup mặc định của SQL Server |
| Xóa nhầm dữ liệu hoặc mất cả database | Chạy `Database/restore_mystore.ps1`: tự lấy bản backup mới nhất và ghi đè lên `MyStore` (kể cả khi DB đã bị xóa) |
| Máy mới hoặc chưa có backup | Tạo DB `MyStore`, rồi chạy `database2024.sql` và `update_mystore.sql`. Chỉ có dữ liệu mẫu |

Máy dùng instance khác thì thêm tham số, ví dụ `-Server "TEN_MAY\SQLEXPRESS"`.

## Cấu trúc chính
```
Areas/Admin/            Khu vực quản trị (Controllers, Views, Filters/AdminAuthorize)
Controllers/            Home, Account, Cart, Order (phần Customer)
Models/                 EDMX + entity, Metadata (Data Annotation), ViewModel (giỏ hàng, tìm kiếm, ...)
Views/Shared/           _CustomerLayout, các partial dùng chung (_ProductInfo, _OrderInvoice, ...)
Content/images/books/   Ảnh bìa sách minh họa
Database/               Script tạo bảng + dữ liệu mẫu
```

## Ghi chú
- Mật khẩu đang lưu dạng chữ thường để giống bài thực hành. Nếu dùng thật thì cần mã hóa (hash).
- Ảnh bìa trong `Content/images/books` là ảnh minh họa tự thiết kế. Có thể thay bằng ảnh thật qua Admin → Sản phẩm → Sửa → Tải ảnh lên.
