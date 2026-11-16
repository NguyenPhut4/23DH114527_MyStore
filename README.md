# FASTOR Books – Website bán sách (ASP.NET MVC5 + Entity Framework 6)

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
