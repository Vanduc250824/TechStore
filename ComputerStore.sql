-- 1. Tạo bảng Categories
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- 2. Tạo bảng Products
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    CategoryID INT FOREIGN KEY REFERENCES Categories(CategoryID) ON DELETE CASCADE,
    Price DECIMAL(10,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(255),
    Description NVARCHAR(1000)
);

-- 3. Tạo bảng Promotions
CREATE TABLE Promotions (
    PromotionID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    DiscountPercent DECIMAL(5,2) CHECK (DiscountPercent BETWEEN 0 AND 100),
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Description NVARCHAR(1000)
);

-- 4. Bảng ProductPromotions (Liên kết Products & Promotions)
CREATE TABLE ProductPromotions (
    ProductPromotionID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID) ON DELETE CASCADE,
    PromotionID INT FOREIGN KEY REFERENCES Promotions(PromotionID) ON DELETE CASCADE
);

-- 5. Bảng Orders (Phải tạo trước khi OrderDetails)
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID NVARCHAR(450) NOT NULL, -- Liên kết với ASP.NET Identity
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Shipped', 'Delivered', 'Cancelled')),
    PromotionID INT FOREIGN KEY REFERENCES Promotions(PromotionID) ON DELETE SET NULL,

    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserID) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- 6. Bảng OrderDetails (Phải tạo sau Orders)
CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT FOREIGN KEY REFERENCES Orders(OrderID) ON DELETE CASCADE,
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID) ON DELETE CASCADE,
    Quantity INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);
Select * from AspNetUsers
SELECT * FROM Categories 
Select * from Products
INSERT INTO Categories (Name) 
VALUES 
    ('Laptops'),
    ('Desktop PCs'),
    ('Networking Devices'),
    ('Printer & Scanners'),
    ('PC Parts'),
    ('All Other Products');
SELECT * FROM Categories
SELECT * FROM Products
SELECT * FROM Promotions;
SELECT * FROM ProductPromotions
SELECT * FROM promotions WHERE Name IS NULL OR Description IS NULL OR StartDate IS NULL OR EndDate IS NULL;
ALTER TABLE Products 
ALTER COLUMN Description NVARCHAR(MAX);

ALTER TABLE Products
ADD 
    OriginalPrice DECIMAL(10,2) NULL,    
    DiscountedPrice DECIMAL(10,2) NULL; 
ALTER TABLE Products
DROP COLUMN Price;
ALTER Table Products ADD ProductType NVARCHAR(MAX)


