create database CashewPro_DB

USE CashewPro_DB;
GO

-- 1. Roles table simplify
ALTER TABLE Roles DROP COLUMN RoleNameTe, RoleNameHi;
EXEC sp_rename 'Roles.RoleNameEn', 'RoleName', 'COLUMN';

-- 2. Sheds table simplify
ALTER TABLE Sheds DROP COLUMN ShedNameTe, ShedNameHi;
EXEC sp_rename 'Sheds.ShedNameEn', 'ShedName', 'COLUMN';

-- 3. ProductGrades table simplify
ALTER TABLE ProductGrades DROP COLUMN GradeNameTe, GradeNameHi;
EXEC sp_rename 'ProductGrades.GradeNameEn', 'GradeName', 'COLUMN';
GO

USE CashewPro_DB;
GO

-- Director (Admin)
INSERT INTO Users (RoleId, FullName, Email, PhoneNumber, PasswordHash, PreferredLanguage, IsActive)
VALUES (1, 'K. Suresh (Director)', 'director@cashewpro.com', '9876543210', 'admin@123', 'te', 1);

-- Shed-1 Supervisor
INSERT INTO Users (RoleId, FullName, Email, PhoneNumber, PasswordHash, PreferredLanguage, IsActive)
VALUES (4, 'Ramesh (Palasa Shed-1)', 'ramesh.shed1@cashewpro.com', '9123456780', 'shed@123', 'te', 1);
GO


--17.10.2026
USE CashewPro_DB;
GO

-- 1. Add UpdatedAt column to DailyCuttingEntries (if not exists)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DailyCuttingEntries') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE DailyCuttingEntries ADD UpdatedAt DATETIME2 NULL;
END
GO

-- 2. Add UpdatedAt to Labours (if not exists)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Labours') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Labours ADD UpdatedAt DATETIME2 NULL;
END
GO

-- 3. Default Supplier & RcnProcurement (BoilingBatch FK support kosam)
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Suppliers ON;
    INSERT INTO Suppliers (Id, SupplierType, FullName, PhoneNumber, Country)
    VALUES (1, 'LOCAL_MANDI', N'Palasa APMC Mandi', '9848011111', 'India');
    SET IDENTITY_INSERT Suppliers OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM RcnProcurements WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT RcnProcurements ON;
    INSERT INTO RcnProcurements 
    (Id, LotNumber, SupplierId, OriginMandi, VehicleNumber, BagsCount, GrossWeightKg, TareWeightKg, NetWeightKg, RatePerQuintal, TotalProcurementCost, ProcurementDate)
    VALUES 
    (1, 'LOT-2026-DEFAULT', 1, N'Palasa Mandi', 'AP30TX1234', 100, 8050.000, 50.000, 8000.000, 12500.00, 1000000.00, CAST(GETDATE() AS DATE));
    SET IDENTITY_INSERT RcnProcurements OFF;
END
GO

-- 4. Default BoilingBatch record (Id = 1)
IF NOT EXISTS (SELECT 1 FROM BoilingBatches WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT BoilingBatches ON;
    INSERT INTO BoilingBatches 
    (Id, BatchNumber, RcnProcurementId, RawBagsUsed, RawWeightInputKg, BoilingDate, Status)
    VALUES 
    (1, 'BATCH-STEAM-001', 1, 20, 1600.000, CURRENT_TIMESTAMP, 'READY_FOR_CUTTING');
    SET IDENTITY_INSERT BoilingBatches OFF;
END
GO

-- 1. Add CreatedAt & UpdatedAt to FinishedStock table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FinishedStock') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE FinishedStock ADD CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FinishedStock') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE FinishedStock ADD UpdatedAt DATETIME2 NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Orders ADD CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Orders ADD UpdatedAt DATETIME2 NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EwayBills') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE EwayBills ADD CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EwayBills') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE EwayBills ADD UpdatedAt DATETIME2 NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderItems') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE OrderItems ADD CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderItems') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE OrderItems ADD UpdatedAt DATETIME2 NULL;
END
GO

-- 1. Ensure UpdatedAt exists on Customers table
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Customers ADD UpdatedAt DATETIME2 NULL;
END
GO

-- 2. Ensure CreatedAt exists with default
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Customers ADD CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
END
GO

-- 3. Ensure columns allow safe lengths
ALTER TABLE Customers ALTER COLUMN PostalCode VARCHAR(30) NULL;
ALTER TABLE Customers ALTER COLUMN City NVARCHAR(150) NULL;
ALTER TABLE Customers ALTER COLUMN BillingAddress NVARCHAR(500) NULL;
ALTER TABLE Customers ALTER COLUMN ShippingAddress NVARCHAR(500) NULL;
GO


-- 1. Labours table lo WorkType mariyu WageBasis column add cheyali
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Labours') AND name = 'WorkType')
BEGIN
    ALTER TABLE Labours ADD WorkType VARCHAR(50) NOT NULL DEFAULT 'CUTTING';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Labours') AND name = 'WageBasis')
BEGIN
    ALTER TABLE Labours ADD WageBasis VARCHAR(20) NOT NULL DEFAULT 'PER_KG'; -- 'PER_KG', 'PER_DAY'
END
GO

-- 2. DailyWorkEntries (Cutting kakunda Peeling, Boiling, Packing, Driver work logging kosam)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DailyOtherWorkEntries')
BEGIN
    CREATE TABLE DailyOtherWorkEntries (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        LabourId BIGINT NOT NULL FOREIGN KEY REFERENCES Labours(Id),
        ShedId INT NOT NULL FOREIGN KEY REFERENCES Sheds(Id),
        WorkType VARCHAR(50) NOT NULL, -- 'PEELING', 'BOILING', 'PACKING', 'LOGISTICS'
        EntryDate DATE NOT NULL,
        QuantityWorked DECIMAL(12, 3) NOT NULL, -- If Per_KG: kgs; If Per_Day: 1.0 or 0.5 (half day)
        UnitRate DECIMAL(10, 2) NOT NULL,
        TotalEarnings DECIMAL(14, 2) NOT NULL,
        Notes NVARCHAR(250) NULL,
        EnteredByUserId BIGINT NOT NULL FOREIGN KEY REFERENCES Users(Id),
        CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt DATETIME2 NULL
    );
END
GO



-- 1. Ensure LabourPayments has CreatedAt & UpdatedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LabourPayments') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE LabourPayments ADD CreatedAt DATETIME2 NOT NULL DEFAULT CURRENT_TIMESTAMP;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LabourPayments') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE LabourPayments ADD UpdatedAt DATETIME2 NULL;
END
GO

-- 2. Alter PaymentDate to DATETIME2 to accept UtcNow without conversion issues
ALTER TABLE LabourPayments ALTER COLUMN PaymentDate DATETIME2 NOT NULL;
ALTER TABLE LabourPayments ALTER COLUMN TransactionRefNumber VARCHAR(150) NULL;
ALTER TABLE LabourPayments ALTER COLUMN Notes NVARCHAR(500) NULL;
GO

-- 3. Verify Users table has at least Id = 1 (Fallback Admin)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Users ON;
    INSERT INTO Users (Id, RoleId, FullName, PhoneNumber, PasswordHash, PreferredLanguage, IsActive)
    VALUES (1, 1, 'System Director', '9876543210', 'admin@123', 'te', 1);
    SET IDENTITY_INSERT Users OFF;
END
GO