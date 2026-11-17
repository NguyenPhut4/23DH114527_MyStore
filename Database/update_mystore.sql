-- ============================================================
-- MyStore: bo sung cot + du lieu mau
-- Chay: sqlcmd -S "DESKTOP-TM3KS1E\LAPTOP" -E -C -d MyStore -f 65001 -i Database\update_mystore.sql
-- Tai khoan test: admin / 123456 (A); khach1, khach2, khach3 / 123456 (C)
-- ============================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 1. Bo sung / doi ten cot (theo database2024 moi va slide p3)
-- Product: ProductDecription -> ProductDescription
IF COL_LENGTH('dbo.Product','ProductDecription') IS NOT NULL AND COL_LENGTH('dbo.Product','ProductDescription') IS NULL
    EXEC sp_rename 'dbo.Product.ProductDecription', 'ProductDescription', 'COLUMN';
IF COL_LENGTH('dbo.Product','Quantity') IS NULL
    ALTER TABLE dbo.Product ADD Quantity INT NOT NULL CONSTRAINT DF_Product_Quantity DEFAULT 0;
IF COL_LENGTH('dbo.Product','SoldQuantity') IS NULL
    ALTER TABLE dbo.Product ADD SoldQuantity INT NOT NULL CONSTRAINT DF_Product_SoldQuantity DEFAULT 0;

-- Order: AddressDelivery -> ShippingAddress; them OrderStatus, PaymentMethod, DeliveryMethod
IF COL_LENGTH('dbo.Order','AddressDelivery') IS NOT NULL AND COL_LENGTH('dbo.Order','ShippingAddress') IS NULL
    EXEC sp_rename 'dbo.Order.AddressDelivery', 'ShippingAddress', 'COLUMN';
IF COL_LENGTH('dbo.Order','ShippingMethod') IS NOT NULL AND COL_LENGTH('dbo.Order','DeliveryMethod') IS NULL
    EXEC sp_rename 'dbo.Order.ShippingMethod', 'DeliveryMethod', 'COLUMN';
IF COL_LENGTH('dbo.Order','OrderStatus') IS NULL
    ALTER TABLE dbo.[Order] ADD OrderStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Order_OrderStatus DEFAULT N'Chờ thanh toán';
IF COL_LENGTH('dbo.Order','PaymentMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD PaymentMethod NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.Order','DeliveryMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD DeliveryMethod NVARCHAR(100) NULL;

-- OrderDetail: TotalPrice tu tinh = Quantity * UnitPrice
IF COL_LENGTH('dbo.OrderDetail','TotalPrice') IS NULL
    ALTER TABLE dbo.OrderDetail ADD TotalPrice AS (Quantity * UnitPrice) PERSISTED;

ALTER TABLE dbo.[User] ALTER COLUMN [Password] NVARCHAR(50) NOT NULL;
GO

-- 2. Du lieu mau (chi chen khi DB con trong)
IF NOT EXISTS (SELECT 1 FROM dbo.[User])
BEGIN
    INSERT INTO dbo.[User] (Username, [Password], UserRole) VALUES
    (N'admin', N'123456', N'A'),
    (N'khach1', N'123456', N'C'),
    (N'khach2', N'123456', N'C'),
    (N'khach3', N'123456', N'C');

    INSERT INTO dbo.Customer (CustomerName, CustomerPhone, CustomerEmail, CustomerAddress, Username) VALUES
    (N'Nguyễn Văn An',  N'0901234567', N'an@example.com',  N'12 Lê Lợi, Quận 1, TP.HCM',        N'khach1'),
    (N'Trần Thị Bình',  N'0912345678', N'binh@example.com', N'45 Nguyễn Huệ, Quận 1, TP.HCM',    N'khach2'),
    (N'Lê Hoàng Cường', N'0987654321', N'cuong@example.com', N'78 Võ Văn Tần, Quận 3, TP.HCM',  N'khach3');

    INSERT INTO dbo.Category (CategoryName) VALUES
    (N'Kinh tế'), (N'Kỹ năng sống'), (N'Văn học'), (N'Thiếu nhi');

    INSERT INTO dbo.Product (CategoryID, ProductName, ProductDescription, ProductPrice, ProductImage, Quantity, SoldQuantity) VALUES
    (1, N'Trò Chơi Của Những Kẻ Hiểu Luật', N'Sách kinh tế về chiến lược và luật chơi trong kinh doanh.', 106548, NULL, 20, 0),
    (1, N'Kinh Doanh Thời Trang - 8 Bước Xây Dựng Shop Quần Áo', N'Hướng dẫn xây dựng, vận hành và phát triển shop thời trang.', 142000, NULL, 15, 0),
    (1, N'Làm Giàu Từ Siêu Cổ Phiếu', N'Câu chuyện đầu tư chứng khoán của Jesse C. Stine.', 399000, NULL, 8, 0),
    (2, N'Nghệ Thuật Từ Chối', N'Cách nói không mà vẫn có được đồng thuận.', 55900, NULL, 30, 0),
    (2, N'Đắc Nhân Tâm', N'Nghệ thuật thu phục lòng người.', 86000, NULL, 50, 0),
    (2, N'Hãy Chăm Sóc Mẹ', N'Tiểu thuyết cảm động về tình mẹ (tái bản 2019).', 67000, NULL, 0, 0),
    (3, N'Cây Cam Ngọt Của Tôi', N'Tiểu thuyết kinh điển của José Mauro de Vasconcelos.', 73800, NULL, 25, 0),
    (3, N'Nhà Giả Kim', N'Hành trình theo đuổi ước mơ của chàng chăn cừu Santiago.', 79000, NULL, 40, 0),
    (3, N'Rừng Na Uy', N'Tiểu thuyết của Haruki Murakami.', 120000, NULL, 0, 0),
    (4, N'Dế Mèn Phiêu Lưu Ký', N'Tác phẩm thiếu nhi kinh điển của Tô Hoài.', 45000, NULL, 60, 0),
    (4, N'Hoàng Tử Bé', N'Câu chuyện triết lý dành cho mọi lứa tuổi.', 52000, NULL, 35, 0),
    (4, N'Doraemon Tập 1', N'Truyện tranh Doraemon.', 22000, NULL, 100, 0);

    -- 25 don hang trai deu 5 trang thai, 3 khach hang, 12 thang gan day
    DECLARE @i INT = 1, @orderId INT, @status NVARCHAR(50), @cust INT, @date DATE;
    WHILE @i <= 25
    BEGIN
        SET @status = CHOOSE((@i - 1) % 5 + 1, N'Chờ thanh toán', N'Đang xử lý', N'Đang vận chuyển', N'Đã giao', N'Đã hủy');
        SET @cust = (SELECT MIN(CustomerID) FROM dbo.Customer) + (@i - 1) % 3;
        SET @date = DATEADD(DAY, -(@i * 13), CAST(GETDATE() AS DATE));

        INSERT INTO dbo.[Order] (CustomerID, OrderDate, TotalAmount, PaymentStatus, ShippingAddress, OrderStatus, PaymentMethod, DeliveryMethod)
        SELECT @cust, @date, 0,
               CASE WHEN @status IN (N'Chờ thanh toán', N'Đã hủy') THEN N'Chưa thanh toán' ELSE N'Đã thanh toán' END,
               c.CustomerAddress, @status,
               CASE WHEN @i % 2 = 0 THEN N'Thanh toán tiền mặt' ELSE N'Chuyển khoản' END,
               CASE WHEN @i % 3 = 0 THEN N'Giao hàng tiết kiệm' ELSE N'Giao hàng nhanh' END
        FROM dbo.Customer c WHERE c.CustomerID = @cust;
        SET @orderId = SCOPE_IDENTITY();

        INSERT INTO dbo.OrderDetail (ProductID, OrderID, Quantity, UnitPrice)
        SELECT p.ProductID, @orderId, 1 + (p.ProductID + @i) % 3, p.ProductPrice
        FROM dbo.Product p
        WHERE (p.ProductID + @i) % 4 = 0;

        UPDATE dbo.[Order]
        SET TotalAmount = (SELECT SUM(Quantity * UnitPrice) FROM dbo.OrderDetail WHERE OrderID = @orderId)
        WHERE OrderID = @orderId;

        SET @i += 1;
    END

    -- so luong da ban = tong so luong trong cac don khong bi huy
    UPDATE p SET SoldQuantity = ISNULL(x.Sold, 0)
    FROM dbo.Product p
    LEFT JOIN (SELECT od.ProductID, SUM(od.Quantity) AS Sold
               FROM dbo.OrderDetail od JOIN dbo.[Order] o ON o.OrderID = od.OrderID
               WHERE o.OrderStatus <> N'Đã hủy'
               GROUP BY od.ProductID) x ON x.ProductID = p.ProductID;
END
GO

-- 3. Anh bia sach (Content/images/books) - chi gan cho san pham chua co anh
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/tro-choi-cua-nhung-ke-hieu-luat.svg' WHERE ProductName = N'Trò Chơi Của Những Kẻ Hiểu Luật' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/kinh-doanh-thoi-trang.svg' WHERE ProductName = N'Kinh Doanh Thời Trang - 8 Bước Xây Dựng Shop Quần Áo' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/lam-giau-tu-sieu-co-phieu.svg' WHERE ProductName = N'Làm Giàu Từ Siêu Cổ Phiếu' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/nghe-thuat-tu-choi.svg' WHERE ProductName = N'Nghệ Thuật Từ Chối' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/dac-nhan-tam.svg' WHERE ProductName = N'Đắc Nhân Tâm' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/hay-cham-soc-me.svg' WHERE ProductName = N'Hãy Chăm Sóc Mẹ' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/cay-cam-ngot-cua-toi.svg' WHERE ProductName = N'Cây Cam Ngọt Của Tôi' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/nha-gia-kim.svg' WHERE ProductName = N'Nhà Giả Kim' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/rung-na-uy.svg' WHERE ProductName = N'Rừng Na Uy' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/de-men-phieu-luu-ky.svg' WHERE ProductName = N'Dế Mèn Phiêu Lưu Ký' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/hoang-tu-be.svg' WHERE ProductName = N'Hoàng Tử Bé' AND ProductImage IS NULL;
UPDATE dbo.Product SET ProductImage = N'~/Content/images/books/doraemon-tap-1.svg' WHERE ProductName = N'Doraemon Tập 1' AND ProductImage IS NULL;
GO

-- 4. Rang buoc du lieu (CHECK / UNIQUE) - chan du lieu sai ngay o tang DB,
--    ke ca khi sua truc tiep bang SSMS. Chay lai nhieu lan van an toan.
SET QUOTED_IDENTIFIER ON;
IF OBJECT_ID('dbo.CK_Product_Price') IS NULL
    ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_Price CHECK (ProductPrice > 0);
IF OBJECT_ID('dbo.CK_Product_Quantity') IS NULL
    ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_Quantity CHECK (Quantity >= 0);
IF OBJECT_ID('dbo.CK_Product_Sold') IS NULL
    ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_Sold CHECK (SoldQuantity >= 0);
IF OBJECT_ID('dbo.CK_OrderDetail_Quantity') IS NULL
    ALTER TABLE dbo.OrderDetail ADD CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0);
IF OBJECT_ID('dbo.CK_OrderDetail_UnitPrice') IS NULL
    ALTER TABLE dbo.OrderDetail ADD CONSTRAINT CK_OrderDetail_UnitPrice CHECK (UnitPrice >= 0);
IF OBJECT_ID('dbo.CK_Order_Total') IS NULL
    ALTER TABLE dbo.[Order] ADD CONSTRAINT CK_Order_Total CHECK (TotalAmount >= 0);
IF OBJECT_ID('dbo.CK_Order_Status') IS NULL
    ALTER TABLE dbo.[Order] ADD CONSTRAINT CK_Order_Status
        CHECK (OrderStatus IN (N'Chờ thanh toán', N'Đang xử lý', N'Đang vận chuyển', N'Đã giao', N'Đã hủy'));
IF OBJECT_ID('dbo.CK_Order_PaymentStatus') IS NULL
    ALTER TABLE dbo.[Order] ADD CONSTRAINT CK_Order_PaymentStatus
        CHECK (PaymentStatus IS NULL OR PaymentStatus IN (N'Chưa thanh toán', N'Đã thanh toán'));
IF OBJECT_ID('dbo.CK_User_Role') IS NULL
    ALTER TABLE dbo.[User] ADD CONSTRAINT CK_User_Role CHECK (UserRole IN (N'A', N'C'));
IF OBJECT_ID('dbo.UQ_Customer_Username') IS NULL
    ALTER TABLE dbo.Customer ADD CONSTRAINT UQ_Customer_Username UNIQUE (Username);
IF OBJECT_ID('dbo.UQ_Customer_Phone') IS NULL
    ALTER TABLE dbo.Customer ADD CONSTRAINT UQ_Customer_Phone UNIQUE (CustomerPhone);
GO
