namespace CashewPro.Domain.Entities;

public class ProductGrade
{
    public int Id { get; set; }
    public string GradeCode { get; set; } = string.Empty;
    public string GradeName { get; set; } = string.Empty;
    public string GradeCategory { get; set; } = "WHOLE_WHITE";
    public string? StandardDescription { get; set; }
    public decimal CurrentRetailPricePerKg { get; set; }
    public decimal CurrentWholesalePricePerKg { get; set; }
    public decimal MinimumWholesaleQtyKg { get; set; } = 50.00m;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}