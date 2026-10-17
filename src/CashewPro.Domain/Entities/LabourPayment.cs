using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class LabourPayment : BaseEntity
{
    public long LabourId { get; set; }
    public Labour Labour { get; set; } = null!;

    public int ShedId { get; set; }
    public Shed Shed { get; set; } = null!;

    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMode { get; set; } = "CASH"; // CASH, UPI, BANK_TRANSFER
    public string? TransactionRefNumber { get; set; }

    public long ProcessedByUserId { get; set; }
    public User ProcessedByUser { get; set; } = null!;

    public string? Notes { get; set; }
}