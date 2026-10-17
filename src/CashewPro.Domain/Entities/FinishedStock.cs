using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class FinishedStock : BaseEntity
{
    public int GradeId { get; set; }
    public ProductGrade Grade { get; set; } = null!;
    public string PackageType { get; set; } = "10KG_TIN";
    public int TotalQuantityCount { get; set; }
    public decimal TotalWeightInKg { get; set; }
    public DateTime LastAuditedAt { get; set; } = DateTime.UtcNow;
}