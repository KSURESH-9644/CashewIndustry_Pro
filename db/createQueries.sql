-- 1. Roles (Director, AsstDirector, Accountant, ShedSupervisor, SalesExecutive, Customer)
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- In PostgreSQL use: SERIAL PRIMARY KEY
    RoleCode VARCHAR(50) NOT NULL UNIQUE,
    RoleNameEn NVARCHAR(100) NOT NULL,
    RoleNameTe NVARCHAR(100) NOT NULL,
    RoleNameHi NVARCHAR(100) NOT NULL,
    Description NVARCHAR(250) NULL,
    IsActive BIT DEFAULT 1 -- PostgreSQL: BOOLEAN DEFAULT TRUE
);

-- 2. Users (Staff and Registered Customers)
CREATE TABLE Users (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY, -- PostgreSQL: BIGSERIAL PRIMARY KEY
    RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(Id),
    FullName NVARCHAR(150) NOT NULL,
    Email VARCHAR(150) NULL UNIQUE,
    PhoneNumber VARCHAR(20) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    PreferredLanguage VARCHAR(10) DEFAULT 'te', -- 'en', 'te', 'hi'
    ProfileImageUrl NVARCHAR(500) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME2 NULL
);

-- 3. Sheds (Distributed Village Cutting & Shelling Units)
CREATE TABLE Sheds (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ShedCode VARCHAR(50) NOT NULL UNIQUE, -- e.g., 'SHED-VLG-01'
    ShedNameEn NVARCHAR(150) NOT NULL,
    ShedNameTe NVARCHAR(150) NOT NULL,
    ShedNameHi NVARCHAR(150) NOT NULL,
    SupervisorUserId BIGINT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    VillageName NVARCHAR(100) NOT NULL,
    Mandal NVARCHAR(100) NOT NULL,
    District NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Latitude DECIMAL(9, 6) NULL, -- For Google Maps Integration
    Longitude DECIMAL(9, 6) NULL,
    DailyCuttingTargetKg DECIMAL(12, 3) DEFAULT 500.000,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 4. Labours (Rural Workers assigned to Sheds)
CREATE TABLE Labours (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    LabourCode VARCHAR(50) NOT NULL UNIQUE, -- e.g., 'LAB-00104'
    ShedId INT NOT NULL FOREIGN KEY REFERENCES Sheds(Id),
    FullName NVARCHAR(150) NOT NULL,
    AadhaarNumber VARCHAR(20) NULL,
    MobileNumber VARCHAR(20) NOT NULL,
    PreferredLanguage VARCHAR(10) DEFAULT 'te', -- 'te', 'hi', 'en'
    PieceRatePerKg DECIMAL(10, 2) NOT NULL DEFAULT 25.00, -- Base rate per cut kg
    CurrentBalanceAmount DECIMAL(14, 2) DEFAULT 0.00, -- Pending payments
    IsActive BIT DEFAULT 1,
    JoinedDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 5. Labour Payments (Settlement history)
CREATE TABLE LabourPayments (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    LabourId BIGINT NOT NULL FOREIGN KEY REFERENCES Labours(Id),
    ShedId INT NOT NULL FOREIGN KEY REFERENCES Sheds(Id),
    PaymentDate DATE NOT NULL,
    AmountPaid DECIMAL(14, 2) NOT NULL,
    PaymentMode VARCHAR(30) NOT NULL, -- 'CASH', 'UPI', 'BANK_TRANSFER'
    TransactionRefNumber VARCHAR(100) NULL,
    ProcessedByUserId BIGINT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Notes NVARCHAR(250) NULL,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 6. Farmers / Vendors (Sources of RCN)
CREATE TABLE Suppliers (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    SupplierType VARCHAR(30) NOT NULL, -- 'LOCAL_FARMER', 'MANDI_TRADER', 'IMPORT_AGENCY'
    FullName NVARCHAR(150) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    Village NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    Country VARCHAR(50) DEFAULT 'India',
    GstNumber VARCHAR(20) NULL,
    BankAccountNumber VARCHAR(50) NULL,
    IfscCode VARCHAR(20) NULL,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 7. RCN Procurement Batches (Inward Trucks/Bags)
CREATE TABLE RcnProcurements (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    LotNumber VARCHAR(50) NOT NULL UNIQUE, -- e.g., 'LOT-2026-OCT-01'
    SupplierId BIGINT NOT NULL FOREIGN KEY REFERENCES Suppliers(Id),
    OriginMandi NVARCHAR(100) NOT NULL,
    VehicleNumber VARCHAR(50) NOT NULL,
    DriverName NVARCHAR(100) NULL,
    DriverPhone VARCHAR(20) NULL,
    BagsCount INT NOT NULL,
    GrossWeightKg DECIMAL(12, 3) NOT NULL,
    TareWeightKg DECIMAL(12, 3) NOT NULL,
    NetWeightKg DECIMAL(12, 3) NOT NULL, -- Exact seed weight
    MoisturePercentage DECIMAL(5, 2) NULL,
    NutCountPerKg INT NULL, -- Quality metric: Nuts per Kg (e.g., 180 - 200)
    OutturnLbs DECIMAL(5, 2) NULL, -- Quality metric: Outturn (e.g., 48 - 52 lbs)
    RatePerQuintal DECIMAL(14, 2) NOT NULL,
    TotalProcurementCost DECIMAL(14, 2) NOT NULL,
    ProcurementDate DATE NOT NULL,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 8. Boiling & Steaming Batches (Central Factory)
CREATE TABLE BoilingBatches (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    BatchNumber VARCHAR(50) NOT NULL UNIQUE,
    RcnProcurementId BIGINT NOT NULL FOREIGN KEY REFERENCES RcnProcurements(Id),
    RawBagsUsed INT NOT NULL,
    RawWeightInputKg DECIMAL(12, 3) NOT NULL,
    BoilingDate DATETIME2 NOT NULL,
    BoilingPressurePsi DECIMAL(5, 2) NULL,
    SteamingDurationMinutes INT NULL,
    CoolingCompletionTime DATETIME2 NULL,
    Status VARCHAR(30) DEFAULT 'READY_FOR_CUTTING' -- 'BOILED', 'COOLED', 'DISPATCHED_TO_SHEDS'
);

-- 9. Daily Cutting Entries (Distributed Village Shed Record - Gram Precision)
CREATE TABLE DailyCuttingEntries (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    BoilingBatchId BIGINT NOT NULL FOREIGN KEY REFERENCES BoilingBatches(Id),
    ShedId INT NOT NULL FOREIGN KEY REFERENCES Sheds(Id),
    LabourId BIGINT NOT NULL FOREIGN KEY REFERENCES Labours(Id),
    EntryDate DATE NOT NULL,
    RawSeedsIssuedKg DECIMAL(12, 3) NOT NULL, -- Weight handed over to worker
    KernelCutWeightKg DECIMAL(12, 3) NOT NULL, -- Actual edible kernel cut output (Kg + Grams)
    UncutOrWasteWeightKg DECIMAL(12, 3) NOT NULL, -- Shells / damaged / rejected
    CalculatedRatePerKg DECIMAL(10, 2) NOT NULL, -- Rate at that time
    TotalEarnings DECIMAL(14, 2) NOT NULL, -- KernelCutWeightKg * CalculatedRatePerKg
    EnteredByUserId BIGINT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    IsWhatsAppNotified BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 10. Product Grades Master (W-180, W-210, W-240, W-320, JH, Pieces)
CREATE TABLE ProductGrades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    GradeCode VARCHAR(30) NOT NULL UNIQUE, -- 'W-180', 'W-240', 'W-320', 'JH', 'LWP'
    GradeNameEn NVARCHAR(100) NOT NULL,
    GradeNameTe NVARCHAR(100) NOT NULL,
    GradeNameHi NVARCHAR(100) NOT NULL,
    GradeCategory VARCHAR(50) NOT NULL, -- 'WHOLE_WHITE', 'SCORCHED_WHOLE', 'SPLITS', 'BROKEN'
    StandardDescription NVARCHAR(500) NULL,
    CurrentRetailPricePerKg DECIMAL(14, 2) NOT NULL,
    CurrentWholesalePricePerKg DECIMAL(14, 2) NOT NULL,
    MinimumWholesaleQtyKg DECIMAL(10, 2) DEFAULT 50.00,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT DEFAULT 1
);

-- 11. Finished Inventory Stock (Warehouse Ground Stock)
CREATE TABLE FinishedStock (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    GradeId INT NOT NULL FOREIGN KEY REFERENCES ProductGrades(Id),
    PackageType VARCHAR(50) NOT NULL, -- '10KG_TIN', '500G_POUCH', '1KG_VACUUM', '50KG_BULK_BAG'
    TotalQuantityCount INT NOT NULL DEFAULT 0,
    TotalWeightInKg DECIMAL(14, 3) NOT NULL DEFAULT 0.000,
    LastAuditedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 12. Daily Market Prices Log (Live Ticker Board on Storefront)
CREATE TABLE DailyMarketPrices (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    PriceDate DATE NOT NULL,
    GradeId INT NOT NULL FOREIGN KEY REFERENCES ProductGrades(Id),
    MandiMarketName NVARCHAR(100) DEFAULT 'Palasa Market',
    PricePerKg DECIMAL(14, 2) NOT NULL,
    PriceTrend VARCHAR(10) NULL, -- 'UP', 'DOWN', 'STABLE'
    RecordedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 13. Customer Master (Storefront retail and wholesale buyers)
CREATE TABLE Customers (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NULL FOREIGN KEY REFERENCES Users(Id), -- Linked if registered
    CustomerType VARCHAR(30) NOT NULL, -- 'RETAIL', 'WHOLESALE_SHOP', 'EXPORT_COMPANY'
    BusinessName NVARCHAR(200) NULL,
    ContactPerson NVARCHAR(150) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Email VARCHAR(150) NULL,
    GstNumber VARCHAR(20) NULL,
    BillingAddress NVARCHAR(500) NOT NULL,
    ShippingAddress NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    PostalCode VARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 14. Orders (Cart / Wholesale Orders)
CREATE TABLE Orders (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL UNIQUE, -- e.g., 'ORD-20261016-0042'
    CustomerId BIGINT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    OrderDate DATETIME2 DEFAULT CURRENT_TIMESTAMP,
    SubTotal DECIMAL(14, 2) NOT NULL,
    TaxAmount DECIMAL(14, 2) NOT NULL, -- CGST + SGST or IGST
    FreightShippingCost DECIMAL(14, 2) DEFAULT 0.00,
    GrandTotal DECIMAL(14, 2) NOT NULL,
    PaymentStatus VARCHAR(30) DEFAULT 'PENDING', -- 'PENDING', 'PAID', 'FAILED'
    OrderStatus VARCHAR(30) DEFAULT 'PLACED', -- 'PLACED', 'PROCESSING', 'DISPATCHED', 'DELIVERED', 'CANCELLED'
    OrderedVia VARCHAR(20) DEFAULT 'WEB_STORE', -- 'WEB_STORE', 'VOICE_BOT', 'MANUAL_SALES'
    CreatedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 15. Order Line Items
CREATE TABLE OrderItems (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL FOREIGN KEY REFERENCES Orders(Id),
    GradeId INT NOT NULL FOREIGN KEY REFERENCES ProductGrades(Id),
    Quantity INT NOT NULL,
    WeightPerUnitKg DECIMAL(10, 3) NOT NULL,
    TotalWeightKg DECIMAL(12, 3) NOT NULL,
    UnitPrice DECIMAL(14, 2) NOT NULL,
    LineTotal DECIMAL(14, 2) NOT NULL
);

-- 16. E-Way Bills & Logistics
CREATE TABLE EwayBills (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL FOREIGN KEY REFERENCES Orders(Id),
    EwayBillNumber VARCHAR(50) NOT NULL UNIQUE,
    VehicleNumber VARCHAR(50) NOT NULL,
    TransporterName NVARCHAR(150) NOT NULL,
    TransporterId VARCHAR(50) NULL,
    ApproxDistanceKm INT NOT NULL,
    GeneratedDate DATETIME2 NOT NULL,
    ValidUntilDate DATETIME2 NOT NULL,
    QrCodeImageUrl NVARCHAR(500) NULL
);

-- 17. Customer & Google Reviews
CREATE TABLE Reviews (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    CustomerId BIGINT NULL FOREIGN KEY REFERENCES Customers(Id),
    CustomerName NVARCHAR(150) NOT NULL,
    Source VARCHAR(30) DEFAULT 'PLATFORM', -- 'GOOGLE_REVIEW', 'PLATFORM_STORE'
    RatingStars INT NOT NULL CHECK (RatingStars BETWEEN 1 AND 5),
    ReviewComment NVARCHAR(1000) NULL,
    ReviewLanguage VARCHAR(10) DEFAULT 'en',
    IsApprovedForDisplay BIT DEFAULT 1,
    ReviewedAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);

-- 18. Notification Logs (WhatsApp / SMS Delivery Status)
CREATE TABLE NotificationLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    RecipientPhone VARCHAR(20) NOT NULL,
    RecipientType VARCHAR(30) NOT NULL, -- 'LABOUR', 'FARMER', 'CUSTOMER'
    Channel VARCHAR(20) NOT NULL, -- 'WHATSAPP', 'SMS'
    Language VARCHAR(10) NOT NULL, -- 'te', 'hi', 'en'
    MessageBody NVARCHAR(1000) NOT NULL,
    Status VARCHAR(30) DEFAULT 'SENT', -- 'QUEUED', 'SENT', 'DELIVERED', 'FAILED'
    SentAt DATETIME2 DEFAULT CURRENT_TIMESTAMP
);