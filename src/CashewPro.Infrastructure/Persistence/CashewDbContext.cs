using CashewPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.Infrastructure.Persistence;

public class CashewDbContext : DbContext
{
    public CashewDbContext(DbContextOptions<CashewDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Shed> Sheds => Set<Shed>();
    public DbSet<Labour> Labours => Set<Labour>();
    public DbSet<LabourPayment> LabourPayments => Set<LabourPayment>();
    public DbSet<DailyCuttingEntry> DailyCuttingEntries => Set<DailyCuttingEntry>();
    public DbSet<ProductGrade> ProductGrades => Set<ProductGrade>();
    public DbSet<DailyMarketPrice> DailyMarketPrices => Set<DailyMarketPrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role table mapping
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoleName).HasMaxLength(100).IsRequired();
        });

        // User table mapping
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

        // Shed table mapping
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

        // Labour table mapping
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

        // Labour Payments
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

        // Daily Cutting Entries (Gram precision: 12, 3)
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

        // Product Grades
        modelBuilder.Entity<ProductGrade>(entity =>
        {
            entity.ToTable("ProductGrades");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GradeCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.CurrentRetailPricePerKg).HasPrecision(14, 2);
            entity.Property(e => e.CurrentWholesalePricePerKg).HasPrecision(14, 2);
            entity.Property(e => e.MinimumWholesaleQtyKg).HasPrecision(10, 2);
        });

        // Daily Market Prices
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
    }
}