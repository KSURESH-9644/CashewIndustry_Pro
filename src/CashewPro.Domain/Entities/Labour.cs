using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class Labour : BaseEntity
{
    public string LabourCode { get; set; } = string.Empty;
    public int ShedId { get; set; }
    public Shed Shed { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string? AadhaarNumber { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "te";
    public decimal PieceRatePerKg { get; set; } = 25.00m;
    public decimal CurrentBalanceAmount { get; set; } = 0.00m;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedDate { get; set; }

    public ICollection<DailyCuttingEntry> CuttingEntries { get; set; } = new List<DailyCuttingEntry>();
    public ICollection<LabourPayment> Payments { get; set; } = new List<LabourPayment>();
}