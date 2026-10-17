using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class BoilingBatch : BaseEntity
{
    public string BatchNumber { get; set; } = string.Empty;
    public long RcnProcurementId { get; set; }
    public RcnProcurement RcnProcurement { get; set; } = null!;

    public int RawBagsUsed { get; set; }
    public decimal RawWeightInputKg { get; set; }
    public DateTime BoilingDate { get; set; } = DateTime.UtcNow;
    public decimal? BoilingPressurePsi { get; set; }
    public int? SteamingDurationMinutes { get; set; }
    public DateTime? CoolingCompletionTime { get; set; }
    public string Status { get; set; } = "READY_FOR_CUTTING";
}