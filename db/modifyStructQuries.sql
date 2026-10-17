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