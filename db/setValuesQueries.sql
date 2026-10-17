USE CashewPro_DB;
GO

-- 1. Default Roles
INSERT INTO Roles (RoleCode, RoleNameEn, RoleNameTe, RoleNameHi, Description)
VALUES 
('SUPER_ADMIN', 'Director / Owner', N'డైరెక్టర్ / యజమాని', N'निदेशक / मालिक', 'Full system access & financial controls'),
('ASST_DIRECTOR', 'Asst. Director / Manager', N'అసిస్టెంట్ డైరెక్టర్ / మేనేజర్', N'सहायक निदेशक / प्रबंधक', 'Operational view & approvals'),
('ACCOUNTANT', 'Accountant', N'అకౌంటెంట్', N'लेखापाल', 'Farmer payouts, GST & labour billing'),
('SHED_SUPERVISOR', 'Shed In-charge', N'షెడ్ ఇన్-ఛార్జ్', N'शेड प्रभारी', 'Daily cutting & labour data entry'),
('CUSTOMER', 'Customer / Buyer', N'కస్టమర్ / కొనుగోలుదారు', N'ग्राहक / खरीदार', 'Public storefront user');

-- 2. Standard Cashew Grades Master (with Multilingual Names & Baseline Rates)
INSERT INTO ProductGrades 
(GradeCode, GradeNameEn, GradeNameTe, GradeNameHi, GradeCategory, CurrentRetailPricePerKg, CurrentWholesalePricePerKg, MinimumWholesaleQtyKg)
VALUES 
('W-180', 'King Jumbo Whole (W-180)', N'కింగ్ జంబో హోల్ (W-180)', N'किंग जंबो काजू (W-180)', 'WHOLE_WHITE', 1180.00, 1050.00, 50.00),
('W-210', 'Jumbo Whole (W-210)', N'జంబో హోల్ (W-210)', N'जंबो काजू (W-210)', 'WHOLE_WHITE', 1050.00, 940.00, 50.00),
('W-240', 'Standard Whole (W-240)', N'స్టాండర్డ్ హోల్ (W-240)', N'स्टैंडर्ड काजू (W-240)', 'WHOLE_WHITE', 950.00, 840.00, 50.00),
('W-320', 'Regular Whole (W-320)', N'రెగ్యులర్ హోల్ (W-320)', N'रेगुलर काजू (W-320)', 'WHOLE_WHITE', 840.00, 750.00, 50.00),
('JH', 'Splits / Split Kernels (JH)', N'జీడి బద్దలు / స్ప్లిట్స్ (JH)', N'काजू टुकड़ा / स्प्लिट्स (JH)', 'SPLITS', 720.00, 640.00, 50.00),
('LWP', 'Large White Pieces (LWP)', N'పెద్ద ముక్కలు (LWP)', N'बड़े टुकड़े (LWP)', 'BROKEN', 650.00, 580.00, 50.00);

-- 3. Sample Live Ticker Entry for Home Screen
INSERT INTO DailyMarketPrices (PriceDate, GradeId, MandiMarketName, PricePerKg, PriceTrend)
VALUES 
(CAST(GETDATE() AS DATE), 1, N'Palasa Cashew Market', 1050.00, 'UP'),
(CAST(GETDATE() AS DATE), 2, N'Palasa Cashew Market', 940.00, 'STABLE'),
(CAST(GETDATE() AS DATE), 3, N'Palasa Cashew Market', 840.00, 'STABLE'),
(CAST(GETDATE() AS DATE), 4, N'Palasa Cashew Market', 750.00, 'DOWN');

USE CashewPro_DB;
GO

-- 1. Insert Village Sheds (Palasa area rural units)
INSERT INTO Sheds (ShedCode, ShedName, SupervisorUserId, VillageName, Mandal, District, State, Latitude, Longitude, DailyCuttingTargetKg, IsActive)
VALUES 
('SHED-PAL-01', 'Brahmana Tarla Shed', 2, 'Brahmana Tarla', 'Palasa', 'Srikakulam', 'Andhra Pradesh', 18.775200, 84.412500, 600.000, 1),
('SHED-PAL-02', 'Kasibugga Rural Shed', 2, 'Kasibugga', 'Palasa', 'Srikakulam', 'Andhra Pradesh', 18.761000, 84.418000, 500.000, 1),
('SHED-MAN-01', 'Mandasa Coastal Shed', 2, 'Mandasa', 'Mandasa', 'Srikakulam', 'Andhra Pradesh', 18.871000, 84.472000, 750.000, 1);
GO

-- 2. Insert Sample Labours for Brahmana Tarla Shed (ShedId = 1)
INSERT INTO Labours (LabourCode, ShedId, FullName, AadhaarNumber, MobileNumber, PreferredLanguage, PieceRatePerKg, CurrentBalanceAmount, IsActive, JoinedDate)
VALUES 
('LAB-101', 1, N'కె. లక్ష్మి (K. Lakshmi)', '789456123011', '9848012345', 'te', 26.00, 0.00, 1, '2026-01-10'),
('LAB-102', 1, N'ఎం. గౌరి (M. Gouri)', '789456123012', '9848012346', 'te', 26.00, 0.00, 1, '2026-01-15'),
('LAB-103', 1, N'పి. సరస్వతి (P. Saraswathi)', '789456123013', '9848012347', 'te', 26.00, 0.00, 1, '2026-02-01'),
('LAB-104', 1, N'బి. రాజేశ్వరి (B. Rajeshwari)', '789456123014', '9848012348', 'te', 26.00, 0.00, 1, '2026-02-10');
GO