# Kết quả kiểm tra Checklist BE + Đề Ktra BE

Kiểm tra ngày 16/11/2026 trên project `NguyenTanPhu_23DH114527` với DB thật `MyStore` (`DESKTOP-TM3KS1E\LAPTOP`).
Cách kiểm tra: build bằng MSBuild, biên dịch toàn bộ 59 view, chạy IIS Express và gửi request HTTP thật tới từng trang và từng form (GET + POST).
Kết quả: **151/151 bước đạt**. Dữ liệu test được xóa sau khi chạy xong.

Tài khoản test: admin `admin` / `123456` · khách hàng `khach1` / `123456` (ghi trong `Database/update_mystore.sql`).

## 1. Đề kiểm tra (Ktra BE.pptx)

| Yêu cầu | Đạt | Nằm ở đâu |
|---|---|---|
| Admin dùng Admin Layout | ✅ | `Areas/Admin/Views/Shared/_AdminLayout.cshtml` |
| Customer dùng Customer Layout | ✅ | `Views/Shared/_CustomerLayout.cshtml` |
| Đăng nhập Admin, menu **Đơn hàng** → Danh sách đơn hàng | ✅ | `Areas/Admin/Controllers/OrdersController.cs` |
| Tìm theo Tên khách hàng, Trạng thái thanh toán, Ngày đặt hàng | ✅ | `Orders/Index.cshtml` |
| Không tìm → hiện toàn bộ, **15 đơn/trang** | ✅ | `OrderSearchVM.PageSize = 15` |
| Sắp xếp | ✅ | bấm tiêu đề cột Ngày / Tên KH / Tổng giá trị |
| Không có kết quả → **KHÔNG CÓ** | ✅ | `PVOrderList.cshtml` |
| Link Chi tiết đơn hàng → **DetailedOrder.cshtml** | ✅ | `Orders/DetailedOrder.cshtml` |
| Nút Cập nhật trạng thái → trang Cập nhật trạng thái | ✅ | `Orders/UpdateStatus.cshtml` (hủy đơn → trả hàng về kho) |
| Trang Home Admin = thống kê | ✅ | `Areas/Admin/Views/Home/Index.cshtml` (Google Chart) |
| Menu Tài khoản → **Lịch sử mua hàng** → Đơn hàng của tôi | ✅ | `Views/Shared/_PersonalMenuPV.cshtml` |
| **MyOrder.cshtml**: tìm theo Mã đơn hàng hoặc Tên sản phẩm | ✅ | `Views/Order/MyOrder.cshtml` |
| Chưa có đơn → "BẠN CHƯA CÓ ĐƠN HÀNG NÀO. HÃY ĐẶT ĐƠN NGAY." + link HOME | ✅ | `MyOrder.cshtml` |
| Tab Tất cả đơn gom nhóm theo trạng thái + 5 tab trạng thái | ✅ | `OrderController.MyOrder` |
| Mỗi trạng thái 2 đơn gần nhất, **Xem thêm** → tối đa 50 đơn | ✅ | `RecentPerStatus = 2`, `MaxPerStatus = 50` |
| **Mua lại** → thêm vào giỏ với số lượng = 1 | ✅ | `OrderController.BuyAgain` |
| **Xem chi tiết** → trang Xem chi tiết sản phẩm | ✅ | link tới `Home/ProductDetail` |

## 2. Checklist (LTW_Checklist_BE.xlsx)

### DB
| Tiêu chí | Đạt | Ghi chú |
|---|---|---|
| Dùng DB thực hành, bổ sung trường dữ liệu và Quantity | ✅ | Thêm `Product.Quantity`, `SoldQuantity`; `Order.OrderStatus`, `PaymentMethod`, `DeliveryMethod`; `OrderDetail.TotalPrice` |
| Phân tích theo thông tin trang tham khảo | ✅ | Thêm trạng thái đơn, hình thức thanh toán / giao hàng theo trang mẫu |
| Có khóa FK tới các bảng liên quan | ✅ | Giữ nguyên FK của database2024 |

### Layout
| Tiêu chí | Đạt | Ghi chú |
|---|---|---|
| Trang Admin đã ghép layout | ✅ | `_AdminLayout.cshtml` (theo slide p2) |
| Trang Customer đã ghép layout | ✅ | `_CustomerLayout.cshtml` (slide p3 đặt tên này thay cho `_MasterLayout`) |

### Admin
| Nhóm | Tiêu chí | Đạt |
|---|---|---|
| Quản lý danh mục | Gom bảng phụ vào sub-menu Quản lý danh mục | ✅ |
| | Create/Edit có Data Annotation | ✅ (`Models/Metadata/ModelMetadata.cs`) |
| | Details có danh sách sản phẩm liên quan | ✅ |
| | Bảng < 5 cột thì bỏ Detail | ⚠️ Giữ Details vì tiêu chí trên (0.25đ) yêu cầu Details có danh sách sản phẩm |
| | Delete kiểm tra ràng buộc | ✅ còn sản phẩm thì không cho xóa |
| | Index hiện tổng số hàng hóa | ✅ |
| Quản lý hàng hóa | Sub-menu Quản lý hàng hóa | ✅ |
| | Index 4-5 cột chính (tên, ảnh, giá, tồn, chủng loại) | ✅ |
| | Detail giống trang Detail của Customer, không có nút MUA HÀNG | ✅ `PVProductInfo` (Details/Delete) gọi partial dùng chung `Views/Shared/_ProductInfo.cshtml` |
| | Create/Edit có Data Annotation | ✅ |
| | Upload ảnh sản phẩm | ✅ lưu vào `Content/images`, chỉ nhận file ảnh ≤ 5MB |
| | Delete kiểm tra ràng buộc | ✅ đã có trong đơn hàng thì không cho xóa |
| Quản lý kho | Danh sách hàng tồn / hàng cần nhập | ✅ `Products/Stock` + partial `PVStockList` |
| Quản lý người dùng | Index chỉ tài khoản quản trị, Thêm/Sửa/Xóa | ✅ |
| | Create/Edit có Data Annotation | ✅ |
| | < 5 cột bỏ Detail | ✅ không có Details |
| Quản lý khách hàng | Index không có Thêm/Sửa/Xóa | ✅ |
| | Detail: thông tin KH + danh sách đơn hàng (Partial của Order Index) | ✅ `PVOrderList` dùng chung với `Orders/Index` |
| Quản lý đơn hàng | Index không có Thêm/Sửa/Xóa | ✅ |
| | Detail dạng hóa đơn, Partial chi tiết đơn hàng | ✅ `_OrderInvoice` + `_OrderDetailList` |

### Customer
| Nhóm | Tiêu chí | Đạt |
|---|---|---|
| Home | Menu lấy theo Category | ✅ |
| | Chỉ hiện sản phẩm tồn > 0 | ✅ |
| | Xem chi tiết / MUA sản phẩm | ✅ |
| | Query đúng mục đích (top 10 bán chạy, 20 sp ít người mua, 6/trang) | ✅ |
| Product Category | Tìm kiếm, phân trang | ✅ |
| | Lọc theo tiêu chí (từ khóa, khoảng giá, sắp xếp) | ✅ |
| | Tồn > 0, xem chi tiết, MUA | ✅ |
| Product Detail | Đủ thông tin, MUA, truyền số lượng sang giỏ | ✅ |
| | Sau khi thêm → chuyển tới giỏ hàng | ✅ |
| Giỏ hàng | Icon giỏ trên menu + số lượng | ✅ |
| | Kiểm tra số lượng (không < 1, không > tồn) | ✅ |
| | Cập nhật tổng tiền, xóa sản phẩm | ✅ |
| | Link Tiếp tục mua hàng, Thanh toán; ẩn Thanh toán khi giỏ rỗng | ✅ |
| Đăng ký | Data Annotation + kiểm tra trùng tài khoản / SĐT | ✅ |
| | Thất bại có thông báo, thành công về trang Đăng nhập | ✅ |
| Đăng nhập | Thất bại có thông báo | ✅ |
| | Thành công: đổi icon Hồ sơ, ẩn Đăng ký / Đăng nhập, hiện tên | ✅ |
| | Truyền thông tin người dùng vào phần thanh toán | ✅ |
| | Dưới icon Hồ sơ có Lịch sử giao dịch và Hồ sơ khách hàng | ✅ "Lịch sử mua hàng" + "Thông tin tài khoản" (theo tên trong đề Ktra / slide p3) |
| Thanh toán | Ẩn nút khi giỏ rỗng | ✅ |
| | Lưu vào Order và OrderDetail, có tổng tiền và thành tiền | ✅ |
| | Tính lại số lượng tồn và số lượng đã bán | ✅ |
| Lịch sử giao dịch | Link chuyển tới trang lịch sử | ✅ |
| | Chỉ đơn của chính khách hàng | ✅ đơn của khách khác trả về 404 |
| | Xem chi tiết = hóa đơn, chung Partial với Admin | ✅ |

## 3. Chưa làm
- Thanh toán PayPal (slide p3 ghi là tùy chọn).
