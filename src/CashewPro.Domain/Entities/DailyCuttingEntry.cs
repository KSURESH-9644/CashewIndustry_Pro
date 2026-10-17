using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class DailyCuttingEntry : BaseEntity
{
    public long BoilingBatchId { get; set; }
    public int ShedId { get; set; }
    public Shed Shed { get; set; } = null!;

    public long LabourId { get; set; }
    public Labour Labour { get; set; } = null!;

    public DateTime EntryDate { get; set; }
    public decimal RawSeedsIssuedKg { get; set; }
    public decimal KernelCutWeightKg { get; set; } // Kg + Grams precision
    public decimal UncutOrWasteWeightKg { get; set; }
    public decimal CalculatedRatePerKg { get; set; }
    public decimal TotalEarnings { get; set; }

    public long EnteredByUserId { get; set; }
    public User EnteredByUser { get; set; } = null!;

    public bool IsWhatsAppNotified { get; set; } = false;
}