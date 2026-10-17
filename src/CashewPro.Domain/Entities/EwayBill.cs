using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class EwayBill : BaseEntity
{
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string EwayBillNumber { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string TransporterName { get; set; } = string.Empty;
    public string? TransporterId { get; set; }
    public int ApproxDistanceKm { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntilDate { get; set; }
    public string? QrCodeImageUrl { get; set; }
}

public class Review : BaseEntity
{
    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Source { get; set; } = "PLATFORM";
    public int RatingStars { get; set; }
    public string? ReviewComment { get; set; }
    public string ReviewLanguage { get; set; } = "en";
    public bool IsApprovedForDisplay { get; set; } = true;
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
}