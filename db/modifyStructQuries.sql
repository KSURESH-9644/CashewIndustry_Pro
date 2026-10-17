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