using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class Customer : BaseEntity
{
    public long? UserId { get; set; }
    public User? User { get; set; }
    public string CustomerType { get; set; } = "RETAIL"; // RETAIL, WHOLESALE_SHOP, EXPORT_COMPANY
    public string? BusinessName { get; set; }
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? GstNumber { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FreightShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentStatus { get; set; } = "PENDING";
    public string OrderStatus { get; set; } = "PLACED";
    public string OrderedVia { get; set; } = "WEB_STORE";

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int GradeId { get; set; }
    public ProductGrade Grade { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal WeightPerUnitKg { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}