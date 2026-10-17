namespace CashewPro.Domain.Entities;

public class DailyMarketPrice
{
    public long Id { get; set; }
    public DateTime PriceDate { get; set; }
    public int GradeId { get; set; }
    public ProductGrade Grade { get; set; } = null!;
    public string MandiMarketName { get; set; } = "Palasa Market";
    public decimal PricePerKg { get; set; }
    public string? PriceTrend { get; set; } // UP, DOWN, STABLE
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}