using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class Supplier : BaseEntity
{
    public string SupplierType { get; set; } = "LOCAL_FARMER"; // LOCAL_FARMER, MANDI_TRADER, IMPORT_AGENCY
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Village { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string Country { get; set; } = "India";
    public string? GstNumber { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IfscCode { get; set; }

    public ICollection<RcnProcurement> Procurements { get; set; } = new List<RcnProcurement>();
}