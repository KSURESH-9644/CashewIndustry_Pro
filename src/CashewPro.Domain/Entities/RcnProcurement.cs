using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class RcnProcurement : BaseEntity
{
    public string LotNumber { get; set; } = string.Empty; // e.g. LOT-202610-001
    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public string OriginMandi { get; set; } = "Palasa APMC";
    public string VehicleNumber { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }

    public int BagsCount { get; set; }
    public decimal GrossWeightKg { get; set; }
    public decimal TareWeightKg { get; set; }
    public decimal NetWeightKg { get; set; } // Gross - Tare

    public decimal? MoisturePercentage { get; set; }
    public int? NutCountPerKg { get; set; } // Standard: 180 - 200
    public decimal? OutturnLbs { get; set; } // Standard: 48 - 52 lbs

    public decimal RatePerQuintal { get; set; }
    public decimal TotalProcurementCost { get; set; } // (NetWeightKg / 100) * RatePerQuintal
    public DateTime ProcurementDate { get; set; }
}