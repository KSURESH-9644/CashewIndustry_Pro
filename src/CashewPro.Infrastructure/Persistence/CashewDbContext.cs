using CashewPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.Infrastructure.Persistence;

public class CashewDbContext : DbContext
{
    public CashewDbContext(DbContextOptions<CashewDbContext> options) : base(options)
    {
    }

    // 1. Auth & Staff Management
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    // 2. Sheds & Labour Operations
    public DbSet<Shed> Sheds => Set<Shed>();
    public DbSet<Labour> Labours => Set<Labour>();
    public DbSet<LabourPayment> LabourPayments => Set<LabourPayment>();
    public DbSet<DailyCuttingEntry> DailyCuttingEntries => Set<DailyCuttingEntry>();

    // 3. Raw Nut Procurement & Boiling
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<RcnProcurement> RcnProcurements => Set<RcnProcurement>();
    public DbSet<BoilingBatch> BoilingBatches => Set<BoilingBatch>();

    // 4. Finished Goods & Daily Market Pricing
    public DbSet<ProductGrade> ProductGrades => Set<ProductGrade>();
    public DbSet<DailyMarketPrice> DailyMarketPrices => Set<DailyMarketPrice>();
    public DbSet<FinishedStock> FinishedStocks => Set<FinishedStock>();

    // 5. Storefront Orders, Logistics & Reviews
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<EwayBill> EwayBills => Set<EwayBill>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Role Table Mapping
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoleName).HasMaxLength(100).IsRequired();
        });

        // 2. User Table Mapping
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10).HasDefaultValue("te");

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Shed Table Mapping
        modelBuilder.Entity<Shed>(entity =>
        {
            entity.ToTable("Sheds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShedCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ShedName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.DailyCuttingTargetKg).HasPrecision(12, 3);

            entity.HasOne(e => e.Supervisor)
                  .WithMany()
                  .HasForeignKey(e => e.SupervisorUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 4. Labour Table Mapping
        modelBuilder.Entity<Labour>(entity =>
        {
            entity.ToTable("Labours");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LabourCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PieceRatePerKg).HasPrecision(10, 2);
            entity.Property(e => e.CurrentBalanceAmount).HasPrecision(14, 2);

            entity.HasOne(e => e.Shed)
                  .WithMany(s => s.Labours)
                  .HasForeignKey(e => e.ShedId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. Labour Payments
        modelBuilder.Entity<LabourPayment>(entity =>
        {
            entity.ToTable("LabourPayments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AmountPaid).HasPrecision(14, 2);

            entity.HasOne(e => e.Labour)
                  .WithMany(l => l.Payments)
                  .HasForeignKey(e => e.LabourId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Shed)
                  .WithMany()
                  .HasForeignKey(e => e.ShedId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 6. Suppliers (Farmers & Mandi Vendors)
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SupplierType).HasMaxLength(30).IsRequired();
        });

        // 7. RCN Procurements (Weighbridge Inward)
        modelBuilder.Entity<RcnProcurement>(entity =>
        {
            entity.ToTable("RcnProcurements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LotNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VehicleNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GrossWeightKg).HasPrecision(12, 3);
            entity.Property(e => e.TareWeightKg).HasPrecision(12, 3);
            entity.Property(e => e.NetWeightKg).HasPrecision(12, 3);
            entity.Property(e => e.MoisturePercentage).HasPrecision(5, 2);
            entity.Property(e => e.OutturnLbs).HasPrecision(5, 2);
            entity.Property(e => e.RatePerQuintal).HasPrecision(14, 2);
            entity.Property(e => e.TotalProcurementCost).HasPrecision(14, 2);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.Procurements)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 8. Boiling Batches
        modelBuilder.Entity<BoilingBatch>(entity =>
        {
            entity.ToTable("BoilingBatches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BatchNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RawWeightInputKg).HasPrecision(12, 3);
            entity.Property(e => e.BoilingPressurePsi).HasPrecision(5, 2);
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("READY_FOR_CUTTING");

            entity.HasOne(e => e.RcnProcurement)
                  .WithMany()
                  .HasForeignKey(e => e.RcnProcurementId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 9. Daily Cutting Entries (Gram precision: 12, 3)
        modelBuilder.Entity<DailyCuttingEntry>(entity =>
        {
            entity.ToTable("DailyCuttingEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RawSeedsIssuedKg).HasPrecision(12, 3);
            entity.Property(e => e.KernelCutWeightKg).HasPrecision(12, 3);
            entity.Property(e => e.UncutOrWasteWeightKg).HasPrecision(12, 3);
            entity.Property(e => e.CalculatedRatePerKg).HasPrecision(10, 2);
            entity.Property(e => e.TotalEarnings).HasPrecision(14, 2);

            entity.HasOne(e => e.Shed)
                  .WithMany(s => s.DailyCuttingEntries)
                  .HasForeignKey(e => e.ShedId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Labour)
                  .WithMany(l => l.CuttingEntries)
                  .HasForeignKey(e => e.LabourId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EnteredByUser)
                  .WithMany()
                  .HasForeignKey(e => e.EnteredByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 10. Product Grades Master
        modelBuilder.Entity<ProductGrade>(entity =>
        {
            entity.ToTable("ProductGrades");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GradeCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.GradeName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CurrentRetailPricePerKg).HasPrecision(14, 2);
            entity.Property(e => e.CurrentWholesalePricePerKg).HasPrecision(14, 2);
            entity.Property(e => e.MinimumWholesaleQtyKg).HasPrecision(10, 2);
        });

        // 11. Daily Market Prices Ticker
        modelBuilder.Entity<DailyMarketPrice>(entity =>
        {
            entity.ToTable("DailyMarketPrices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PricePerKg).HasPrecision(14, 2);

            entity.HasOne(e => e.Grade)
                  .WithMany()
                  .HasForeignKey(e => e.GradeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 12. Finished Goods Ground Stock
        modelBuilder.Entity<FinishedStock>(entity =>
        {
            entity.ToTable("FinishedStock");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PackageType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalWeightInKg).HasPrecision(14, 3);

            entity.HasOne(e => e.Grade)
                  .WithMany()
                  .HasForeignKey(e => e.GradeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 13. Customers
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContactPerson).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CustomerType).HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 14. Orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubTotal).HasPrecision(14, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(14, 2);
            entity.Property(e => e.FreightShippingCost).HasPrecision(14, 2);
            entity.Property(e => e.GrandTotal).HasPrecision(14, 2);
            entity.Property(e => e.PaymentStatus).HasMaxLength(30).HasDefaultValue("PENDING");
            entity.Property(e => e.OrderStatus).HasMaxLength(30).HasDefaultValue("PLACED");

            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 15. Order Line Items
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeightPerUnitKg).HasPrecision(10, 3);
            entity.Property(e => e.TotalWeightKg).HasPrecision(12, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(14, 2);
            entity.Property(e => e.LineTotal).HasPrecision(14, 2);

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Grade)
                  .WithMany()
                  .HasForeignKey(e => e.GradeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 16. E-Way Bills
        modelBuilder.Entity<EwayBill>(entity =>
        {
            entity.ToTable("EwayBills");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EwayBillNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.VehicleNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TransporterName).HasMaxLength(150).IsRequired();

            entity.HasOne(e => e.Order)
                  .WithOne()
                  .HasForeignKey<EwayBill>(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 17. Reviews
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(30).HasDefaultValue("PLATFORM");
            entity.Property(e => e.ReviewLanguage).HasMaxLength(10).HasDefaultValue("en");

            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}