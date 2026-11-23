/* =====================================================================
   ĐỒ ÁN LẬP TRÌNH WEB – CƠ SỞ DỮ LIỆU (bản cuối kỳ, đã bổ sung PaymentStatus, DeliveryMethod)
   Đề tài : Xây dựng website bán sách trực tuyến Hiên Sách
   SV     : Nguyễn Tấn Phú – MSSV 23DH114527
   CSDL   : HienSach  (SQL Server 2017 trở lên – dùng STRING_AGG trong view)
   Cách chạy: mở file trong SSMS, bấm Execute (F5) một lần.
   Script tự xoá CSDL cũ (nếu có) rồi tạo lại toàn bộ.
   ===================================================================== */
USE master;
GO
IF DB_ID(N'HienSach') IS NOT NULL
BEGIN
    ALTER DATABASE HienSach SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HienSach;
END
GO
CREATE DATABASE HienSach COLLATE Vietnamese_CI_AS;
GO
USE HienSach;
GO

/* =====================================================================
   PHẦN 1. TẠO BẢNG (khoá chính, NOT NULL, DEFAULT)
   ===================================================================== */
CREATE TABLE dbo.Users (
    UserId        int IDENTITY(1,1) NOT NULL,
    Username      varchar(50) NOT NULL,
    PasswordHash  varchar(255) NOT NULL,
    FullName      nvarchar(100) NOT NULL,
    Email         varchar(100) NOT NULL,
    PhoneNumber   varchar(15) NULL,
    Address       nvarchar(255) NULL,
    Role          varchar(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT ('Customer'),
    CreatedAt     datetime2 NOT NULL
                  CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),
    IsActive      bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CONSTRAINT PK_Users PRIMARY KEY (UserId)
);
CREATE TABLE dbo.Categories (
    CategoryId    int IDENTITY(1,1) NOT NULL,
    CategoryName  nvarchar(100) NOT NULL,
    ParentId      int NULL,
    Description   nvarchar(500) NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId)
);
CREATE TABLE dbo.Publishers (
    PublisherId    int IDENTITY(1,1) NOT NULL,
    PublisherName  nvarchar(150) NOT NULL,
    Address        nvarchar(255) NULL,
    Phone          varchar(15) NULL,
    CONSTRAINT PK_Publishers PRIMARY KEY (PublisherId)
);
CREATE TABLE dbo.Authors (
    AuthorId     int IDENTITY(1,1) NOT NULL,
    AuthorName   nvarchar(100) NOT NULL,
    BirthYear    int NULL,
    Nationality  nvarchar(50) NULL,
    Biography    nvarchar(max) NULL,
    CONSTRAINT PK_Authors PRIMARY KEY (AuthorId)
);
CREATE TABLE dbo.Books (
    BookId         int IDENTITY(1,1) NOT NULL,
    ISBN           varchar(13) NOT NULL,
    Title          nvarchar(250) NOT NULL,
    CategoryId     int NOT NULL,
    PublisherId    int NOT NULL,
    Price          decimal(18,2) NOT NULL,
    StockQuantity  int NOT NULL CONSTRAINT DF_Books_StockQuantity DEFAULT (0),
    PageCount      int NULL,
    PublishYear    int NULL,
    CoverImage     nvarchar(255) NULL,
    Description    nvarchar(max) NULL,
    CreatedAt      datetime2 NOT NULL
                   CONSTRAINT DF_Books_CreatedAt DEFAULT (SYSDATETIME()),
    IsActive       bit NOT NULL CONSTRAINT DF_Books_IsActive DEFAULT (1),
    CONSTRAINT PK_Books PRIMARY KEY (BookId)
);
CREATE TABLE dbo.BookAuthors (
    BookId      int NOT NULL,
    AuthorId    int NOT NULL,
    AuthorRole  nvarchar(30) NOT NULL
                CONSTRAINT DF_BookAuthors_AuthorRole DEFAULT (N'Tác giả'),
    CONSTRAINT PK_BookAuthors PRIMARY KEY (BookId, AuthorId)
);
CREATE TABLE dbo.Coupons (
    CouponId         int IDENTITY(1,1) NOT NULL,
    Code             varchar(20) NOT NULL,
    DiscountPercent  int NOT NULL,
    MaxDiscount      decimal(18,2) NULL,
    MinOrderAmount   decimal(18,2) NOT NULL
                     CONSTRAINT DF_Coupons_MinOrderAmount DEFAULT (0),
    StartDate        datetime2 NOT NULL,
    EndDate          datetime2 NOT NULL,
    UsageLimit       int NOT NULL CONSTRAINT DF_Coupons_UsageLimit DEFAULT (100),
    UsedCount        int NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT (0),
    IsActive         bit NOT NULL CONSTRAINT DF_Coupons_IsActive DEFAULT (1),
    CONSTRAINT PK_Coupons PRIMARY KEY (CouponId)
);
CREATE TABLE dbo.Orders (
    OrderId          int IDENTITY(1,1) NOT NULL,
    UserId           int NOT NULL,
    CouponId         int NULL,
    OrderDate        datetime2 NOT NULL
                     CONSTRAINT DF_Orders_OrderDate DEFAULT (SYSDATETIME()),
    ReceiverName     nvarchar(100) NOT NULL,
    ReceiverPhone    varchar(15) NOT NULL,
    ShippingAddress  nvarchar(255) NOT NULL,
    ShippingFee      decimal(18,2) NOT NULL CONSTRAINT DF_Orders_ShippingFee DEFAULT (0),
    DiscountAmount   decimal(18,2) NOT NULL
                     CONSTRAINT DF_Orders_DiscountAmount DEFAULT (0),
    TotalAmount      decimal(18,2) NOT NULL CONSTRAINT DF_Orders_TotalAmount DEFAULT (0),
    PaymentMethod    varchar(20) NOT NULL
                     CONSTRAINT DF_Orders_PaymentMethod DEFAULT ('COD'),
    Status           varchar(20) NOT NULL
                     CONSTRAINT DF_Orders_Status DEFAULT ('Pending'),
    PaymentStatus    varchar(20) NOT NULL
                     CONSTRAINT DF_Orders_PaymentStatus DEFAULT ('Unpaid'),
    DeliveryMethod   varchar(20) NOT NULL
                     CONSTRAINT DF_Orders_DeliveryMethod DEFAULT ('STANDARD'),
    Note             nvarchar(500) NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (OrderId)
);
CREATE TABLE dbo.OrderItems (
    OrderId    int NOT NULL,
    BookId     int NOT NULL,
    Quantity   int NOT NULL,
    UnitPrice  decimal(18,2) NOT NULL,
    CONSTRAINT PK_OrderItems PRIMARY KEY (OrderId, BookId)
);
CREATE TABLE dbo.CartItems (
    UserId    int NOT NULL,
    BookId    int NOT NULL,
    Quantity  int NOT NULL CONSTRAINT DF_CartItems_Quantity DEFAULT (1),
    AddedAt   datetime2 NOT NULL CONSTRAINT DF_CartItems_AddedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_CartItems PRIMARY KEY (UserId, BookId)
);
CREATE TABLE dbo.Reviews (
    ReviewId    int IDENTITY(1,1) NOT NULL,
    BookId      int NOT NULL,
    UserId      int NOT NULL,
    Rating      int NOT NULL,
    Comment     nvarchar(1000) NULL,
    CreatedAt   datetime2 NOT NULL
                CONSTRAINT DF_Reviews_CreatedAt DEFAULT (SYSDATETIME()),
    IsApproved  bit NOT NULL CONSTRAINT DF_Reviews_IsApproved DEFAULT (0),
    CONSTRAINT PK_Reviews PRIMARY KEY (ReviewId)
);
CREATE TABLE dbo.BookPriceHistory (
    HistoryId  int IDENTITY(1,1) NOT NULL,
    BookId     int NOT NULL,
    OldPrice   decimal(18,2) NOT NULL,
    NewPrice   decimal(18,2) NOT NULL,
    ChangedAt  datetime2 NOT NULL
               CONSTRAINT DF_BookPriceHistory_ChangedAt DEFAULT (SYSDATETIME()),
    ChangedBy  nvarchar(128) NOT NULL
               CONSTRAINT DF_BookPriceHistory_ChangedBy DEFAULT (SUSER_SNAME()),
    CONSTRAINT PK_BookPriceHistory PRIMARY KEY (HistoryId)
);
GO

/* =====================================================================
   PHẦN 2. RÀNG BUỘC: FOREIGN KEY, UNIQUE, CHECK
   ===================================================================== */
-- 2.1 Khoá ngoại
ALTER TABLE dbo.Categories ADD CONSTRAINT FK_Categories_Parent
    FOREIGN KEY (ParentId) REFERENCES dbo.Categories(CategoryId);
ALTER TABLE dbo.Books ADD CONSTRAINT FK_Books_Categories
    FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId);
ALTER TABLE dbo.Books ADD CONSTRAINT FK_Books_Publishers
    FOREIGN KEY (PublisherId) REFERENCES dbo.Publishers(PublisherId);
ALTER TABLE dbo.BookAuthors ADD CONSTRAINT FK_BookAuthors_Books
    FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId) ON DELETE CASCADE;
ALTER TABLE dbo.BookAuthors ADD CONSTRAINT FK_BookAuthors_Authors
    FOREIGN KEY (AuthorId) REFERENCES dbo.Authors(AuthorId);
ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);
ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_Coupons
    FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(CouponId) ON DELETE SET NULL;
ALTER TABLE dbo.OrderItems ADD CONSTRAINT FK_OrderItems_Orders
    FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE;
ALTER TABLE dbo.OrderItems ADD CONSTRAINT FK_OrderItems_Books
    FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId);
ALTER TABLE dbo.CartItems ADD CONSTRAINT FK_CartItems_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE;
ALTER TABLE dbo.CartItems ADD CONSTRAINT FK_CartItems_Books
    FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId) ON DELETE CASCADE;
ALTER TABLE dbo.Reviews ADD CONSTRAINT FK_Reviews_Books
    FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId) ON DELETE CASCADE;
ALTER TABLE dbo.Reviews ADD CONSTRAINT FK_Reviews_Users
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE;
ALTER TABLE dbo.BookPriceHistory ADD CONSTRAINT FK_BookPriceHistory_Books
    FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId) ON DELETE CASCADE;

-- 2.2 UNIQUE
ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Username
    UNIQUE (Username);
ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Email
    UNIQUE (Email);
ALTER TABLE dbo.Categories ADD CONSTRAINT UQ_Categories_CategoryName
    UNIQUE (CategoryName);
ALTER TABLE dbo.Publishers ADD CONSTRAINT UQ_Publishers_PublisherName
    UNIQUE (PublisherName);
ALTER TABLE dbo.Books ADD CONSTRAINT UQ_Books_ISBN
    UNIQUE (ISBN);
ALTER TABLE dbo.Coupons ADD CONSTRAINT UQ_Coupons_Code
    UNIQUE (Code);
ALTER TABLE dbo.Reviews ADD CONSTRAINT UQ_Reviews_User_Book
    UNIQUE (UserId, BookId);

-- 2.3 CHECK
ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role
    CHECK (Role IN ('Customer', 'Staff', 'Admin'));
ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Email
    CHECK (Email LIKE '%_@_%._%');
ALTER TABLE dbo.Categories ADD CONSTRAINT CK_Categories_NotSelfParent
    CHECK (ParentId IS NULL OR ParentId <> CategoryId);
ALTER TABLE dbo.Authors ADD CONSTRAINT CK_Authors_BirthYear
    CHECK (BirthYear BETWEEN 1000 AND YEAR(GETDATE()));
ALTER TABLE dbo.Books ADD CONSTRAINT CK_Books_ISBN
    CHECK (LEN(ISBN) IN (10, 13) AND ISBN NOT LIKE '%[^0-9X]%');
ALTER TABLE dbo.Books ADD CONSTRAINT CK_Books_Price
    CHECK (Price >= 0);
ALTER TABLE dbo.Books ADD CONSTRAINT CK_Books_StockQuantity
    CHECK (StockQuantity >= 0);
ALTER TABLE dbo.Books ADD CONSTRAINT CK_Books_PageCount
    CHECK (PageCount > 0);
ALTER TABLE dbo.Books ADD CONSTRAINT CK_Books_PublishYear
    CHECK (PublishYear BETWEEN 1900 AND YEAR(GETDATE()));
ALTER TABLE dbo.Coupons ADD CONSTRAINT CK_Coupons_DiscountPercent
    CHECK (DiscountPercent BETWEEN 1 AND 100);
ALTER TABLE dbo.Coupons ADD CONSTRAINT CK_Coupons_Dates
    CHECK (EndDate > StartDate);
ALTER TABLE dbo.Coupons ADD CONSTRAINT CK_Coupons_UsedCount
    CHECK (UsedCount >= 0 AND UsedCount <= UsageLimit);
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_Status
    CHECK (Status IN ('Pending', 'Confirmed', 'Shipping', 'Completed', 'Cancelled'));
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_PaymentMethod
    CHECK (PaymentMethod IN ('COD', 'BANKING', 'EWALLET'));
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_Amounts
    CHECK (ShippingFee >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0);
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_PaymentStatus
    CHECK (PaymentStatus IN ('Unpaid', 'Paid'));
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_DeliveryMethod
    CHECK (DeliveryMethod IN ('STANDARD', 'EXPRESS'));
-- Logic trạng thái đơn và thanh toán (xem Models/OrderWorkflow.cs):
--  Pending: chưa thanh toán; Completed: đã thanh toán; trả trước (BANKING, EWALLET) đã xác nhận thì phải Paid;
--  COD chỉ Paid khi đã giao (Completed).
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_StatusPayment CHECK (
        (Status <> 'Pending'   OR PaymentStatus = 'Unpaid')
    AND (Status <> 'Completed' OR PaymentStatus = 'Paid')
    AND (PaymentMethod = 'COD' OR Status NOT IN ('Confirmed', 'Shipping', 'Completed') OR PaymentStatus = 'Paid')
    AND (PaymentMethod <> 'COD' OR Status = 'Completed' OR PaymentStatus = 'Unpaid')
);
ALTER TABLE dbo.OrderItems ADD CONSTRAINT CK_OrderItems_Quantity
    CHECK (Quantity > 0);
ALTER TABLE dbo.OrderItems ADD CONSTRAINT CK_OrderItems_UnitPrice
    CHECK (UnitPrice >= 0);
ALTER TABLE dbo.CartItems ADD CONSTRAINT CK_CartItems_Quantity
    CHECK (Quantity > 0);
ALTER TABLE dbo.Reviews ADD CONSTRAINT CK_Reviews_Rating
    CHECK (Rating BETWEEN 1 AND 5);
GO

/* =====================================================================
   PHẦN 3. TRIGGER
   ===================================================================== */
CREATE TRIGGER dbo.trg_OrderItems_DecreaseStock
ON dbo.OrderItems
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- Gom theo BookId: một lệnh INSERT nhiều dòng có thể chứa cùng một sách
    -- (ở các đơn khác nhau); UPDATE ... JOIN không gom thì chỉ trừ một lần.
    UPDATE b
    SET    b.StockQuantity = b.StockQuantity - x.TotalQty
    FROM   dbo.Books b
    JOIN  (SELECT BookId, SUM(Quantity) AS TotalQty
           FROM inserted
           GROUP BY BookId) x ON b.BookId = x.BookId;
    -- Nếu tồn kho âm, CK_Books_StockQuantity báo lỗi và toàn bộ lệnh INSERT bị huỷ.
END;
GO
CREATE TRIGGER dbo.trg_Orders_StatusChange
ON dbo.Orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(Status) RETURN;

    -- 1) Đơn đã Completed hoặc Cancelled thì không được đổi trạng thái nữa
    IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.OrderId = d.OrderId
               WHERE d.Status IN ('Completed', 'Cancelled') AND i.Status <> d.Status)
        -- THROW trong trigger (XACT_ABORT mặc định ON) huỷ toàn bộ giao dịch
        THROW 50010, N'Đơn hàng đã hoàn tất hoặc đã huỷ, không thể đổi trạng thái.', 1;

    -- 2) Lấy các đơn vừa chuyển sang Cancelled
    DECLARE @Cancelled TABLE (OrderId int PRIMARY KEY, CouponId int NULL);
    INSERT INTO @Cancelled (OrderId, CouponId)
    SELECT i.OrderId, i.CouponId
    FROM inserted i JOIN deleted d ON i.OrderId = d.OrderId
    WHERE i.Status = 'Cancelled' AND d.Status <> 'Cancelled';

    IF NOT EXISTS (SELECT 1 FROM @Cancelled) RETURN;

    -- 3) Hoàn lại tồn kho
    UPDATE b
    SET    b.StockQuantity = b.StockQuantity + x.Qty
    FROM   dbo.Books b
    JOIN  (SELECT oi.BookId, SUM(oi.Quantity) AS Qty
           FROM dbo.OrderItems oi JOIN @Cancelled c ON oi.OrderId = c.OrderId
           GROUP BY oi.BookId) x ON b.BookId = x.BookId;

    -- 4) Trả lại lượt dùng coupon
    UPDATE cp
    SET    cp.UsedCount = cp.UsedCount - x.Cnt
    FROM   dbo.Coupons cp
    JOIN  (SELECT CouponId, COUNT(*) AS Cnt
           FROM @Cancelled WHERE CouponId IS NOT NULL
           GROUP BY CouponId) x ON cp.CouponId = x.CouponId;
END;
GO
CREATE TRIGGER dbo.trg_Books_LogPriceChange
ON dbo.Books
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(Price) RETURN;

    INSERT INTO dbo.BookPriceHistory (BookId, OldPrice, NewPrice)
    SELECT d.BookId, d.Price, i.Price
    FROM   inserted i
    JOIN   deleted  d ON i.BookId = d.BookId
    WHERE  i.Price <> d.Price;      -- chỉ ghi khi giá thực sự thay đổi
END;
GO

/* =====================================================================
   PHẦN 4. VIEW VÀ STORED PROCEDURE
   ===================================================================== */
CREATE VIEW dbo.vw_BookCatalog
AS
SELECT b.BookId, b.ISBN, b.Title,
       c.CategoryName,
       ISNULL(pc.CategoryName, c.CategoryName) AS RootCategory,
       p.PublisherName,
       au.AuthorNames,
       b.Price, b.StockQuantity,
       rv.ReviewCount, rv.AvgRating
FROM   dbo.Books b
JOIN   dbo.Categories c       ON b.CategoryId  = c.CategoryId
LEFT JOIN dbo.Categories pc   ON c.ParentId    = pc.CategoryId
JOIN   dbo.Publishers p       ON b.PublisherId = p.PublisherId
OUTER APPLY (SELECT STRING_AGG(a.AuthorName, N', ') AS AuthorNames
             FROM dbo.BookAuthors ba JOIN dbo.Authors a ON ba.AuthorId = a.AuthorId
             WHERE ba.BookId = b.BookId) au
OUTER APPLY (SELECT COUNT(*) AS ReviewCount,
                    CAST(AVG(CAST(r.Rating AS decimal(3,2))) AS decimal(3,2)) AS AvgRating
             FROM dbo.Reviews r
             WHERE r.BookId = b.BookId AND r.IsApproved = 1) rv
WHERE  b.IsActive = 1;
GO
CREATE PROCEDURE dbo.sp_PlaceOrder
    @UserId          int,
    @ReceiverName    nvarchar(100),
    @ReceiverPhone   varchar(15),
    @ShippingAddress nvarchar(255),
    @PaymentMethod   varchar(20)   = 'COD',
    @ShippingFee     decimal(18,2) = 30000,
    @CouponCode      varchar(20)   = NULL,
    @DeliveryMethod  varchar(20)   = 'STANDARD'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;                     -- lỗi bất kỳ => huỷ toàn bộ giao dịch
    DECLARE @SubTotal decimal(18,2), @CouponId int = NULL,
            @Discount decimal(18,2) = 0, @OrderId int;
    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @SubTotal = SUM(ci.Quantity * b.Price)
        FROM   dbo.CartItems ci JOIN dbo.Books b ON ci.BookId = b.BookId
        WHERE  ci.UserId = @UserId;

        IF @SubTotal IS NULL
            THROW 50001, N'Giỏ hàng trống.', 1;

        SELECT @CouponId = CouponId,
               @Discount = CASE WHEN MaxDiscount IS NOT NULL
                                 AND ROUND(@SubTotal * DiscountPercent / 100.0, 0)
                                     > MaxDiscount
                                THEN MaxDiscount
                                ELSE ROUND(@SubTotal * DiscountPercent / 100.0, 0) END
        FROM   dbo.Coupons
        WHERE  Code = @CouponCode AND IsActive = 1
          AND  SYSDATETIME() BETWEEN StartDate AND EndDate
          AND  UsedCount < UsageLimit AND @SubTotal >= MinOrderAmount;

        INSERT INTO dbo.Orders (UserId, CouponId, ReceiverName, ReceiverPhone,
                    ShippingAddress, ShippingFee, DiscountAmount, TotalAmount,
                    PaymentMethod, DeliveryMethod)
        VALUES (@UserId, @CouponId, @ReceiverName, @ReceiverPhone,
                @ShippingAddress, @ShippingFee, @Discount,
                @SubTotal + @ShippingFee - @Discount, @PaymentMethod, @DeliveryMethod);
        SET @OrderId = SCOPE_IDENTITY();

        -- trigger trg_OrderItems_DecreaseStock tự trừ kho
        INSERT INTO dbo.OrderItems (OrderId, BookId, Quantity, UnitPrice)
        SELECT @OrderId, ci.BookId, ci.Quantity, b.Price
        FROM   dbo.CartItems ci JOIN dbo.Books b ON ci.BookId = b.BookId
        WHERE  ci.UserId = @UserId;

        IF @CouponId IS NOT NULL
            UPDATE dbo.Coupons SET UsedCount = UsedCount + 1
            WHERE  CouponId = @CouponId;

        DELETE FROM dbo.CartItems WHERE UserId = @UserId;

        COMMIT TRANSACTION;
        SELECT @OrderId AS OrderId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

/* =====================================================================
   PHẦN 5. INDEX
   ===================================================================== */
CREATE NONCLUSTERED INDEX IX_Books_Title ON dbo.Books(Title);
CREATE NONCLUSTERED INDEX IX_Books_CategoryId ON dbo.Books(CategoryId)
    INCLUDE (Price, IsActive);
CREATE NONCLUSTERED INDEX IX_BookAuthors_AuthorId ON dbo.BookAuthors(AuthorId);
CREATE NONCLUSTERED INDEX IX_Orders_UserId_OrderDate 
    ON dbo.Orders(UserId, OrderDate DESC);
CREATE NONCLUSTERED INDEX IX_Orders_Status_OrderDate
    ON dbo.Orders(Status, OrderDate);
CREATE NONCLUSTERED INDEX IX_OrderItems_BookId ON dbo.OrderItems(BookId);
CREATE NONCLUSTERED INDEX IX_Reviews_BookId ON dbo.Reviews(BookId)
    INCLUDE (Rating, IsApproved);
GO

/* =====================================================================
   PHẦN 6. DỮ LIỆU MẪU
   (Mật khẩu mẫu là chuỗi băm BCrypt giả lập; ứng dụng băm bằng BCrypt.Net)
   ===================================================================== */
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserId, Username, PasswordHash, FullName, Email, PhoneNumber, Address, Role, CreatedAt, IsActive) VALUES
    (1, 'admin', '$2a$11$S0aM.dRd9vZOWGJmSmq8mevRyEHo9236FouebXsjkUobXuyw6GLBG', N'Quản Trị Viên', 'admin@hiensach.vn', '0901000001', N'828 Sư Vạn Hạnh, P.13, Q.10, TP.HCM', 'Admin', '2026-01-05T08:00:00', 1),
    (2, 'nhanvien01', '$2a$11$cLvOG/zM054Um5Lk1j3aX.Y4JIzsHtQleUa7mlQvdtytfwmKe9XPG', N'Trần Thị Mai', 'mai.tran@hiensach.vn', '0901000002', N'12 Lê Lợi, Q.1, TP.HCM', 'Staff', '2026-01-10T08:30:00', 1),
    (3, 'khoale', '$2a$11$98FujAX2NS.cFhf6x7E7muTMAVIWnlE4yfju/D9JxScI3rP1iL5dC', N'Lê Minh Khoa', 'khoa.le@gmail.com', '0912345601', N'45 Nguyễn Trãi, Q.5, TP.HCM', 'Customer', '2026-03-02T19:20:00', 1),
    (4, 'thuha.pham', '$2a$11$xtoz2I536xacwXFnJAhoX.SDIJvTxmjK2NKvdwClZ6TCFpiQxNseS', N'Phạm Thu Hà', 'thuha.pham@gmail.com', '0912345602', N'102 Cách Mạng Tháng 8, Q.3, TP.HCM', 'Customer', '2026-04-11T10:05:00', 1),
    (5, 'namvo', '$2a$11$6ruKfqlh88hPOfG1l6FjYOC6Wxuyz3nd3xZMehKYkTQ.g6UnDIpa.', N'Võ Hoàng Nam', 'nam.vo@outlook.com', '0912345603', N'7 Phan Văn Trị, Gò Vấp, TP.HCM', 'Customer', '2026-05-20T21:40:00', 1),
    (6, 'lan.dang', '$2a$11$UKG6vLJO8fxQgkxlLZGMd.1NPpiM3rmxy.1ycE5dGj52ACwAZEcWm', N'Đặng Ngọc Lan', 'lan.dang@yahoo.com', '0912345604', N'88 Võ Văn Ngân, TP. Thủ Đức', 'Customer', '2026-06-03T14:15:00', 1),
    (7, 'huybui', '$2a$11$.6e7nxdv4YfEu6cl3yVcCOSBVQXphB3i.WwR67.7Z0ETWFxQdJzje', N'Bùi Quốc Huy', 'huy.bui@gmail.com', '0912345605', N'21 Hoàng Văn Thụ, Tân Bình, TP.HCM', 'Customer', '2026-06-28T09:00:00', 1),
    (8, 'truchuynh', '$2a$11$nbHuJTOQlvXa5EbodT0KLu5jqddtqdnp2aRE9Vw7dD9N2.AoneuYO', N'Huỳnh Thanh Trúc', 'truc.huynh@gmail.com', '0912345606', N'350 Lê Văn Sỹ, Q.3, TP.HCM', 'Customer', '2026-07-15T16:45:00', 1);
SET IDENTITY_INSERT dbo.Users OFF;
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (CategoryId, CategoryName, ParentId, Description) VALUES
    (1, N'Văn học', NULL, N'Sách văn học trong và ngoài nước'),
    (2, N'Kinh tế - Kỹ năng', NULL, N'Sách kinh doanh, phát triển bản thân'),
    (3, N'Khoa học - Công nghệ', NULL, N'Khoa học phổ thông và công nghệ thông tin'),
    (4, N'Thiếu nhi', NULL, N'Sách dành cho thiếu nhi'),
    (5, N'Tiểu thuyết', 1, N'Tiểu thuyết Việt Nam và nước ngoài'),
    (6, N'Truyện ngắn - Tản văn', 1, N'Tập truyện ngắn, tản văn'),
    (7, N'Kỹ năng sống', 2, N'Kỹ năng giao tiếp, phát triển bản thân'),
    (8, N'Quản trị - Kinh doanh', 2, N'Khởi nghiệp, quản trị, làm giàu'),
    (9, N'Lịch sử - Khoa học phổ thông', 3, N'Lịch sử nhân loại, khoa học thường thức'),
    (10, N'Lập trình - CNTT', 3, N'Sách lập trình, kỹ nghệ phần mềm');
SET IDENTITY_INSERT dbo.Categories OFF;
SET IDENTITY_INSERT dbo.Publishers ON;
INSERT INTO dbo.Publishers (PublisherId, PublisherName, Address, Phone) VALUES
    (1, N'NXB Trẻ', N'161B Lý Chính Thắng, Q.3, TP.HCM', '02839316289'),
    (2, N'NXB Kim Đồng', N'55 Quang Trung, Hai Bà Trưng, Hà Nội', '02439434730'),
    (3, N'NXB Hội Nhà Văn', N'65 Nguyễn Du, Hai Bà Trưng, Hà Nội', '02438222135'),
    (4, N'NXB Tổng hợp TP.HCM', N'62 Nguyễn Thị Minh Khai, Q.1, TP.HCM', '02838225340'),
    (5, N'NXB Tri Thức', N'53 Nguyễn Du, Hai Bà Trưng, Hà Nội', '02439454661'),
    (6, N'NXB Thế Giới', N'46 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', '02438253841'),
    (7, N'NXB Thanh Niên', N'64 Bà Triệu, Hoàn Kiếm, Hà Nội', '02439424044');
SET IDENTITY_INSERT dbo.Publishers OFF;
SET IDENTITY_INSERT dbo.Authors ON;
INSERT INTO dbo.Authors (AuthorId, AuthorName, BirthYear, Nationality, Biography) VALUES
    (1, N'Nguyễn Nhật Ánh', 1955, N'Việt Nam', N'Nhà văn chuyên viết cho tuổi mới lớn.'),
    (2, N'Tô Hoài', 1920, N'Việt Nam', N'Tác giả Dế Mèn phiêu lưu ký.'),
    (3, 'Paulo Coelho', 1947, 'Brazil', N'Tiểu thuyết gia người Brazil.'),
    (4, N'Lê Chu Cầu', NULL, N'Việt Nam', N'Dịch giả.'),
    (5, 'Dale Carnegie', 1888, N'Hoa Kỳ', N'Tác giả sách kỹ năng giao tiếp.'),
    (6, N'Rosie Nguyễn', NULL, N'Việt Nam', N'Tác giả sách kỹ năng cho người trẻ.'),
    (7, 'Yuval Noah Harari', 1976, 'Israel', N'Sử gia, giảng viên đại học.'),
    (8, 'Robert C. Martin', 1952, N'Hoa Kỳ', N'Kỹ sư phần mềm, tác giả Clean Code.'),
    (9, 'Andrew Hunt', NULL, N'Hoa Kỳ', N'Đồng tác giả The Pragmatic Programmer.'),
    (10, 'David Thomas', NULL, 'Anh', N'Đồng tác giả The Pragmatic Programmer.'),
    (11, N'Nguyễn Ngọc Tư', 1976, N'Việt Nam', N'Nhà văn Nam Bộ.'),
    (12, 'Napoleon Hill', 1883, N'Hoa Kỳ', N'Tác giả sách làm giàu.');
SET IDENTITY_INSERT dbo.Authors OFF;
SET IDENTITY_INSERT dbo.Books ON;
INSERT INTO dbo.Books (BookId, ISBN, Title, CategoryId, PublisherId, Price, StockQuantity, PageCount, PublishYear, CoverImage, IsActive) VALUES
    (1, '9786041000011', N'Mắt Biếc', 5, 1, 110000, 120, 300, 2019, '/images/books/9786041000011.jpg', 1),
    (2, '9786041000028', N'Cho Tôi Xin Một Vé Đi Tuổi Thơ', 5, 1, 85000, 80, 220, 2018, '/images/books/9786041000028.jpg', 1),
    (3, '9786041000035', N'Tôi Thấy Hoa Vàng Trên Cỏ Xanh', 5, 1, 125000, 100, 378, 2018, '/images/books/9786041000035.jpg', 1),
    (4, '9786042000017', N'Dế Mèn Phiêu Lưu Ký', 4, 2, 60000, 150, 144, 2020, '/images/books/9786042000017.jpg', 1),
    (5, '9786043000016', N'Nhà Giả Kim', 5, 3, 79000, 200, 228, 2020, '/images/books/9786043000016.jpg', 1),
    (6, '9786044000015', N'Đắc Nhân Tâm', 7, 4, 86000, 180, 320, 2021, '/images/books/9786044000015.jpg', 1),
    (7, '9786043000023', N'Tuổi Trẻ Đáng Giá Bao Nhiêu', 7, 3, 90000, 90, 285, 2019, '/images/books/9786043000023.jpg', 1),
    (8, '9786045000014', N'Sapiens: Lược Sử Loài Người', 9, 5, 259000, 60, 556, 2021, '/images/books/9786045000014.jpg', 1),
    (9, '9786047000012', N'Clean Code (Bản tiếng Việt)', 10, 7, 320000, 40, 464, 2022, '/images/books/9786047000012.jpg', 1),
    (10, '9786046000013', N'The Pragmatic Programmer (Bản tiếng Việt)', 10, 6, 299000, 35, 352, 2023, '/images/books/9786046000013.jpg', 1),
    (11, '9786041000042', N'Cánh Đồng Bất Tận', 6, 1, 95000, 70, 216, 2019, '/images/books/9786041000042.jpg', 1),
    (12, '9786046000020', N'Homo Deus: Lược Sử Tương Lai', 9, 6, 279000, 50, 512, 2022, '/images/books/9786046000020.jpg', 1),
    (13, '9786044000022', N'Nghĩ Giàu Và Làm Giàu', 8, 4, 115000, 65, 400, 2020, '/images/books/9786044000022.jpg', 1),
    (14, '9786042000024', N'Kính Vạn Hoa (Tập 1)', 4, 2, 45000, 0, 180, 2015, '/images/books/9786042000024.jpg', 0);
SET IDENTITY_INSERT dbo.Books OFF;
INSERT INTO dbo.BookAuthors (BookId, AuthorId, AuthorRole) VALUES
    (1, 1, N'Tác giả'),
    (2, 1, N'Tác giả'),
    (3, 1, N'Tác giả'),
    (4, 2, N'Tác giả'),
    (5, 3, N'Tác giả'),
    (5, 4, N'Dịch giả'),
    (6, 5, N'Tác giả'),
    (7, 6, N'Tác giả'),
    (8, 7, N'Tác giả'),
    (9, 8, N'Tác giả'),
    (10, 9, N'Đồng tác giả'),
    (10, 10, N'Đồng tác giả'),
    (11, 11, N'Tác giả'),
    (12, 7, N'Tác giả'),
    (13, 12, N'Tác giả'),
    (14, 1, N'Tác giả');
-- UsedCount ban đầu tính cả đơn sẽ bị huỷ ở cuối phần 6 (trigger sẽ trả lại lượt)
SET IDENTITY_INSERT dbo.Coupons ON;
INSERT INTO dbo.Coupons (CouponId, Code, DiscountPercent, MaxDiscount, MinOrderAmount, StartDate, EndDate, UsageLimit, UsedCount, IsActive) VALUES
    (1, 'WELCOME10', 10, 30000, 100000, '2026-01-01T00:00:00', '2026-12-31T23:59:59', 500, 3, 1),
    (2, 'SACH20', 20, 60000, 300000, '2026-06-01T00:00:00', '2026-12-31T23:59:59', 200, 2, 1),
    (3, 'HUFLIT15', 15, 40000, 150000, '2026-09-01T00:00:00', '2026-12-31T23:59:59', 100, 0, 1),
    (4, 'BLACKFRIDAY25', 25, 100000, 200000, '2026-11-20T00:00:00', '2026-11-30T23:59:59', 300, 1, 1),
    (5, 'TET2026', 30, 80000, 250000, '2026-01-20T00:00:00', '2026-02-28T23:59:59', 100, 0, 1);
SET IDENTITY_INSERT dbo.Coupons OFF;
-- Đơn sẽ huỷ được thêm ở trạng thái Pending, cuối phần này UPDATE sang Cancelled để trigger hoàn kho
SET IDENTITY_INSERT dbo.Orders ON;
INSERT INTO dbo.Orders (OrderId, UserId, CouponId, OrderDate, ReceiverName, ReceiverPhone, ShippingAddress, ShippingFee, DiscountAmount, TotalAmount, PaymentMethod, Status, Note, PaymentStatus, DeliveryMethod) VALUES
    (1, 3, NULL, '2026-07-05T10:15:00', N'Lê Minh Khoa', '0912345601', N'45 Nguyễn Trãi, Q.5, TP.HCM', 30000, 0, 219000, 'COD', 'Completed', NULL, 'Paid', 'STANDARD'),
    (2, 4, 1, '2026-07-18T20:40:00', N'Phạm Thu Hà', '0912345602', N'102 Cách Mạng Tháng 8, Q.3, TP.HCM', 30000, 26200, 265800, 'BANKING', 'Completed', N'Gói quà giúp mình', 'Paid', 'STANDARD'),
    (3, 5, NULL, '2026-08-02T09:05:00', N'Võ Hoàng Nam', '0912345603', N'7 Phan Văn Trị, Gò Vấp, TP.HCM', 30000, 0, 289000, 'COD', 'Completed', NULL, 'Paid', 'STANDARD'),
    (4, 6, NULL, '2026-08-15T14:30:00', N'Đặng Ngọc Lan', '0912345604', N'88 Võ Văn Ngân, TP. Thủ Đức', 30000, 0, 425000, 'EWALLET', 'Completed', NULL, 'Paid', 'STANDARD'),
    (5, 7, 2, '2026-08-28T19:10:00', N'Bùi Quốc Huy', '0912345605', N'21 Hoàng Văn Thụ, Tân Bình, TP.HCM', 0, 60000, 559000, 'BANKING', 'Completed', NULL, 'Paid', 'STANDARD'),
    (6, 3, NULL, '2026-09-06T11:00:00', N'Lê Minh Khoa', '0912345601', N'45 Nguyễn Trãi, Q.5, TP.HCM', 30000, 0, 235000, 'COD', 'Completed', NULL, 'Paid', 'STANDARD'),
    (7, 8, 1, '2026-09-19T08:45:00', N'Huỳnh Thanh Trúc', '0912345606', N'350 Lê Văn Sỹ, Q.3, TP.HCM', 30000, 24400, 249600, 'EWALLET', 'Completed', NULL, 'Paid', 'STANDARD'),
    (8, 4, NULL, '2026-09-30T21:20:00', N'Phạm Thu Hà', '0912345602', N'102 Cách Mạng Tháng 8, Q.3, TP.HCM', 0, 0, 538000, 'BANKING', 'Completed', NULL, 'Paid', 'STANDARD'),
    (9, 5, NULL, '2026-10-08T13:35:00', N'Võ Hoàng Nam', '0912345603', N'7 Phan Văn Trị, Gò Vấp, TP.HCM', 30000, 0, 231000, 'COD', 'Completed', NULL, 'Paid', 'STANDARD'),
    (10, 6, 2, '2026-10-21T10:50:00', N'Đặng Ngọc Lan', '0912345604', N'88 Võ Văn Ngân, TP. Thủ Đức', 0, 60000, 684000, 'BANKING', 'Completed', NULL, 'Paid', 'STANDARD'),
    (11, 6, NULL, '2026-10-25T16:00:00', N'Đặng Ngọc Lan', '0912345604', N'88 Võ Văn Ngân, TP. Thủ Đức', 30000, 0, 116000, 'COD', 'Pending', NULL, 'Unpaid', 'STANDARD'),
    (12, 7, NULL, '2026-11-02T18:25:00', N'Bùi Quốc Huy', '0912345605', N'21 Hoàng Văn Thụ, Tân Bình, TP.HCM', 30000, 0, 320000, 'COD', 'Shipping', N'Giao giờ hành chính', 'Unpaid', 'STANDARD'),
    (13, 8, NULL, '2026-11-10T09:30:00', N'Huỳnh Thanh Trúc', '0912345606', N'350 Lê Văn Sỹ, Q.3, TP.HCM', 30000, 0, 215000, 'EWALLET', 'Confirmed', NULL, 'Paid', 'STANDARD'),
    (14, 3, 1, '2026-11-14T22:10:00', N'Lê Minh Khoa', '0912345601', N'45 Nguyễn Trãi, Q.5, TP.HCM', 0, 30000, 508000, 'COD', 'Pending', NULL, 'Unpaid', 'STANDARD'),
    (15, 5, NULL, '2026-11-18T15:40:00', N'Võ Hoàng Nam', '0912345603', N'7 Phan Văn Trị, Gò Vấp, TP.HCM', 30000, 0, 375000, 'COD', 'Pending', NULL, 'Unpaid', 'STANDARD'),
    (16, 4, 4, '2026-11-20T08:30:00', N'Phạm Thu Hà', '0912345602', N'102 Cách Mạng Tháng 8, Q.3, TP.HCM', 30000, 100000, 365000, 'BANKING', 'Pending', NULL, 'Unpaid', 'STANDARD');
SET IDENTITY_INSERT dbo.Orders OFF;
-- Trigger trg_OrderItems_DecreaseStock tự trừ tồn kho khi chạy lệnh này
INSERT INTO dbo.OrderItems (OrderId, BookId, Quantity, UnitPrice) VALUES
    (1, 1, 1, 110000),
    (1, 5, 1, 79000),
    (2, 6, 2, 86000),
    (2, 7, 1, 90000),
    (3, 8, 1, 259000),
    (4, 3, 2, 125000),
    (4, 2, 1, 85000),
    (4, 4, 1, 60000),
    (5, 9, 1, 320000),
    (5, 10, 1, 299000),
    (6, 11, 1, 95000),
    (6, 1, 1, 110000),
    (7, 5, 2, 79000),
    (7, 6, 1, 86000),
    (8, 12, 1, 279000),
    (8, 8, 1, 259000),
    (9, 13, 1, 115000),
    (9, 6, 1, 86000),
    (10, 10, 1, 299000),
    (10, 9, 1, 320000),
    (10, 3, 1, 125000),
    (11, 6, 1, 86000),
    (12, 2, 2, 85000),
    (12, 4, 2, 60000),
    (13, 7, 1, 90000),
    (13, 11, 1, 95000),
    (14, 8, 1, 259000),
    (14, 12, 1, 279000),
    (15, 1, 2, 110000),
    (15, 3, 1, 125000),
    (16, 9, 1, 320000),
    (16, 13, 1, 115000);
INSERT INTO dbo.CartItems (UserId, BookId, Quantity, AddedAt) VALUES
    (3, 4, 1, '2026-11-15T20:01:00'),
    (3, 9, 1, '2026-11-17T21:12:00'),
    (3, 6, 2, '2026-11-19T07:45:00'),
    (4, 2, 1, '2026-11-12T10:00:00'),
    (4, 11, 2, '2026-11-13T11:30:00'),
    (6, 8, 1, '2026-11-16T09:20:00'),
    (6, 12, 1, '2026-11-16T09:22:00'),
    (7, 5, 1, '2026-11-18T22:05:00'),
    (8, 13, 1, '2026-11-19T12:10:00'),
    (8, 1, 1, '2026-11-19T12:12:00');
INSERT INTO dbo.Reviews (BookId, UserId, Rating, Comment, CreatedAt, IsApproved) VALUES
    (1, 3, 5, N'Truyện rất cảm động, đọc lại vẫn thích.', '2026-07-12T20:00:00', 1),
    (5, 3, 4, N'Sách hay, bìa đẹp.', '2026-07-13T08:30:00', 1),
    (6, 4, 5, N'Nên đọc ít nhất một lần.', '2026-07-25T21:00:00', 1),
    (7, 4, 4, N'Phù hợp cho sinh viên.', '2026-07-26T09:15:00', 1),
    (8, 5, 5, N'Kiến thức rộng, dịch tốt.', '2026-08-10T19:45:00', 1),
    (3, 6, 5, N'Văn phong trong trẻo.', '2026-08-22T10:00:00', 1),
    (4, 6, 4, N'Con mình rất thích.', '2026-08-22T10:05:00', 1),
    (9, 7, 5, N'Bắt buộc với lập trình viên.', '2026-09-03T22:30:00', 1),
    (10, 7, 4, N'Nhiều lời khuyên thực tế.', '2026-09-04T07:50:00', 1),
    (11, 3, 4, N'Giọng văn Nam Bộ đặc sắc.', '2026-09-12T18:20:00', 1),
    (12, 4, 3, N'Hơi khó đọc so với Sapiens.', '2026-10-05T20:10:00', 0),
    (13, 5, 4, N'Truyền động lực tốt.', '2026-10-15T08:00:00', 1);
-- Huỷ đơn: trigger trg_Orders_StatusChange hoàn kho + trả lượt coupon
UPDATE dbo.Orders SET Status = 'Cancelled' WHERE OrderId IN (11, 14);
-- Điều chỉnh giá: trigger trg_Books_LogPriceChange ghi vào BookPriceHistory
UPDATE dbo.Books SET Price = 129000 WHERE BookId = 3;
UPDATE dbo.Books SET Price = 89000 WHERE BookId = 6;
UPDATE dbo.Books SET Price = 249000 WHERE BookId = 8;
UPDATE dbo.Books SET Price = 289000 WHERE BookId = 10;
UPDATE dbo.Books SET Price = 269000 WHERE BookId = 12;
GO

/* =====================================================================
   PHẦN 7. KIỂM TRA SỐ DÒNG MỖI BẢNG
   ===================================================================== */
SELECT N'Users' AS TableName, COUNT(*) AS RowCnt FROM dbo.Users
UNION ALL
SELECT N'Categories' AS TableName, COUNT(*) AS RowCnt FROM dbo.Categories
UNION ALL
SELECT N'Publishers' AS TableName, COUNT(*) AS RowCnt FROM dbo.Publishers
UNION ALL
SELECT N'Authors' AS TableName, COUNT(*) AS RowCnt FROM dbo.Authors
UNION ALL
SELECT N'Books' AS TableName, COUNT(*) AS RowCnt FROM dbo.Books
UNION ALL
SELECT N'BookAuthors' AS TableName, COUNT(*) AS RowCnt FROM dbo.BookAuthors
UNION ALL
SELECT N'Coupons' AS TableName, COUNT(*) AS RowCnt FROM dbo.Coupons
UNION ALL
SELECT N'Orders' AS TableName, COUNT(*) AS RowCnt FROM dbo.Orders
UNION ALL
SELECT N'OrderItems' AS TableName, COUNT(*) AS RowCnt FROM dbo.OrderItems
UNION ALL
SELECT N'CartItems' AS TableName, COUNT(*) AS RowCnt FROM dbo.CartItems
UNION ALL
SELECT N'Reviews' AS TableName, COUNT(*) AS RowCnt FROM dbo.Reviews
UNION ALL
SELECT N'BookPriceHistory' AS TableName, COUNT(*) AS RowCnt FROM dbo.BookPriceHistory;
GO

/* =====================================================================
   PHẦN 8. TRUY VẤN CÁC CHỨC NĂNG (MỤC 4 CỦA BÁO CÁO)
   Các chức năng 4.4 và 4.5 làm thay đổi dữ liệu nên để trong chú thích;
   bỏ /* */ để chạy thử.
   ===================================================================== */
-- 4.1. Duyệt sách theo danh mục (gồm cả danh mục con) và khoảng giá
DECLARE @CategoryId int = 1,
        @MinPrice decimal(18,2) = 80000,
        @MaxPrice decimal(18,2) = 130000;

SELECT b.BookId, b.Title, c.CategoryName, p.PublisherName,
       b.Price, b.StockQuantity
FROM   Books b
JOIN   Categories c ON b.CategoryId  = c.CategoryId
JOIN   Publishers p ON b.PublisherId = p.PublisherId
WHERE  b.IsActive = 1
  AND (c.CategoryId = @CategoryId OR c.ParentId = @CategoryId)
  AND  b.Price BETWEEN @MinPrice AND @MaxPrice
ORDER BY b.Price;
GO
-- 4.2. Tìm kiếm sách theo tên sách hoặc tên tác giả, có phân trang
DECLARE @Keyword nvarchar(100) = N'Nguyễn',
        @Page int = 1, @PageSize int = 3;

SELECT b.BookId, b.Title, b.Price,
       COUNT(*) OVER () AS TotalRows          -- tổng số dòng trước khi phân trang
FROM   Books b
WHERE  b.IsActive = 1
  AND (b.Title LIKE N'%' + @Keyword + N'%'
       OR EXISTS (SELECT 1
                  FROM   BookAuthors ba
                  JOIN   Authors a ON ba.AuthorId = a.AuthorId
                  WHERE  ba.BookId = b.BookId
                    AND  a.AuthorName LIKE N'%' + @Keyword + N'%'))
ORDER BY b.Price
OFFSET (@Page - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;
GO
-- 4.3. Xem giỏ hàng và tính tạm tính
DECLARE @UserId int = 3;

SELECT ci.BookId, b.Title, b.Price, ci.Quantity,
       ci.Quantity * b.Price AS LineTotal,
       CAST(CASE WHEN ci.Quantity <= b.StockQuantity THEN 1 ELSE 0 END AS bit)
           AS IsAvailable
FROM   CartItems ci
JOIN   Books b ON ci.BookId = b.BookId
WHERE  ci.UserId = @UserId
ORDER BY ci.AddedAt DESC;

-- Tạm tính của giỏ
SELECT SUM(ci.Quantity * b.Price) AS CartTotal
FROM   CartItems ci JOIN Books b ON ci.BookId = b.BookId
WHERE  ci.UserId = @UserId;
GO
-- 4.4. Đặt hàng từ giỏ hàng (INSERT nhiều bảng trong một transaction)
/*
DECLARE @UserId int = 3, @CouponCode varchar(20) = 'BLACKFRIDAY25',
        @ShippingFee decimal(18,2) = 30000, @PaymentMethod varchar(20) = 'COD',
        @ReceiverName nvarchar(100) = N'Lê Minh Khoa',
        @ReceiverPhone varchar(15) = '0912345601',
        @ShippingAddress nvarchar(255) = N'45 Nguyễn Trãi, Q.5, TP.HCM';
DECLARE @SubTotal decimal(18,2), @CouponId int = NULL,
        @Discount decimal(18,2) = 0, @OrderId int;
BEGIN TRY
    BEGIN TRANSACTION;
    -- (1) Tiền hàng theo giá hiện tại
    SELECT @SubTotal = SUM(ci.Quantity * b.Price)
    FROM   CartItems ci JOIN Books b ON ci.BookId = b.BookId
    WHERE  ci.UserId = @UserId;
    IF @SubTotal IS NULL THROW 50001, N'Giỏ hàng trống.', 1;

    -- (2) Kiểm tra coupon, tính tiền giảm (không có dòng hợp lệ => giữ 0)
    SELECT @CouponId = CouponId,
           @Discount = CASE WHEN MaxDiscount IS NOT NULL
                             AND ROUND(@SubTotal * DiscountPercent / 100.0, 0)
                                 > MaxDiscount
                            THEN MaxDiscount
                            ELSE ROUND(@SubTotal * DiscountPercent / 100.0, 0) END
    FROM   Coupons
    WHERE  Code = @CouponCode AND IsActive = 1
      AND  SYSDATETIME() BETWEEN StartDate AND EndDate
      AND  UsedCount < UsageLimit AND @SubTotal >= MinOrderAmount;

    -- (3) Đầu đơn (OrderDate, Status lấy giá trị DEFAULT)
    INSERT INTO Orders (UserId, CouponId, ReceiverName, ReceiverPhone,
                        ShippingAddress, ShippingFee, DiscountAmount,
                        TotalAmount, PaymentMethod)
    VALUES (@UserId, @CouponId, @ReceiverName, @ReceiverPhone,
            @ShippingAddress, @ShippingFee, @Discount,
            @SubTotal + @ShippingFee - @Discount, @PaymentMethod);
    SET @OrderId = SCOPE_IDENTITY();

    -- (4) Chi tiết đơn (trigger trg_OrderItems_DecreaseStock tự trừ kho)
    INSERT INTO OrderItems (OrderId, BookId, Quantity, UnitPrice)
    SELECT @OrderId, ci.BookId, ci.Quantity, b.Price
    FROM   CartItems ci JOIN Books b ON ci.BookId = b.BookId
    WHERE  ci.UserId = @UserId;

    -- (5) Tăng lượt dùng coupon, (6) xoá giỏ
    IF @CouponId IS NOT NULL
        UPDATE Coupons SET UsedCount = UsedCount + 1 WHERE CouponId = @CouponId;
    DELETE FROM CartItems WHERE UserId = @UserId;

    COMMIT TRANSACTION;
    SELECT @OrderId AS NewOrderId;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
*/
GO
-- 4.5. Huỷ đơn hàng đang chờ xử lý (UPDATE, kích hoạt trigger hoàn kho)
/*
DECLARE @OrderId int = 15, @UserId int = 5;

UPDATE Orders
SET    Status = 'Cancelled'
WHERE  OrderId = @OrderId
  AND  UserId  = @UserId
  AND  Status  = 'Pending';

SELECT @@ROWCOUNT AS RowsAffected;   -- 1: huỷ thành công, 0: không được phép huỷ
*/
GO
-- 4.6. Thống kê doanh thu theo danh mục gốc trong khoảng thời gian
DECLARE @FromDate datetime2 = '2026-07-01', @ToDate datetime2 = '2026-12-01';

SELECT ISNULL(p.CategoryName, c.CategoryName) AS RootCategory,
       COUNT(DISTINCT o.OrderId)              AS OrderCount,
       SUM(oi.Quantity)                       AS QuantitySold,
       SUM(oi.Quantity * oi.UnitPrice)        AS Revenue
FROM   Orders o
JOIN   OrderItems oi     ON o.OrderId    = oi.OrderId
JOIN   Books b           ON oi.BookId    = b.BookId
JOIN   Categories c      ON b.CategoryId = c.CategoryId
LEFT JOIN Categories p   ON c.ParentId   = p.CategoryId
WHERE  o.Status = 'Completed'
  AND  o.OrderDate >= @FromDate AND o.OrderDate < @ToDate
GROUP BY ISNULL(p.CategoryName, c.CategoryName)
ORDER BY Revenue DESC;
GO
-- 4.7. Top 5 tác giả có số lượng sách bán ra nhiều nhất
SELECT TOP 5
       a.AuthorId, a.AuthorName,
       COUNT(DISTINCT oi.BookId) AS TitlesSold,
       SUM(oi.Quantity)          AS TotalQuantity
FROM   Orders o
JOIN   OrderItems  oi ON o.OrderId   = oi.OrderId
JOIN   BookAuthors ba ON oi.BookId   = ba.BookId
JOIN   Authors     a  ON ba.AuthorId = a.AuthorId
WHERE  o.Status = 'Completed'
  AND  ba.AuthorRole <> N'Dịch giả'
GROUP BY a.AuthorId, a.AuthorName
ORDER BY TotalQuantity DESC, a.AuthorId;
GO
-- Xem dữ liệu qua view
SELECT * FROM dbo.vw_BookCatalog ORDER BY BookId;
GO
-- Gọi stored procedure (bỏ chú thích để chạy)
/*
EXEC dbo.sp_PlaceOrder @UserId = 4, @ReceiverName = N'Phạm Thu Hà', @ReceiverPhone = '0912345602',
     @ShippingAddress = N'102 Cách Mạng Tháng 8, Q.3, TP.HCM', @CouponCode = 'HUFLIT15';
*/
GO
