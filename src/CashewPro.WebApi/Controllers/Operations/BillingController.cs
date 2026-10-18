using CashewPro.Domain.Entities;
using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class BillingController : ControllerBase
{
    private readonly CashewDbContext _context;

    public BillingController(CashewDbContext context)
    {
        _context = context;
    }

    [HttpGet("wholesale-customers")]
    public async Task<IActionResult> GetWholesaleCustomers(CancellationToken ct)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.ContactPerson)
            .Select(c => new
            {
                c.Id,
                c.BusinessName,
                c.ContactPerson,
                c.Phone,
                c.GstNumber,
                c.City,
                c.State,
                c.ShippingAddress
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = customers });
    }

    public record InvoiceItemRequest(int GradeId, int QuantityTins, decimal UnitPricePerKg);

    public record GenerateInvoiceRequest(
        long? CustomerId,
        string BusinessName,
        string ContactPerson,
        string Phone,
        string? GstNumber,
        string ShippingAddress,
        string DestinationState,
        string VehicleNumber,
        string TransporterName,
        int ApproxDistanceKm,
        List<InvoiceItemRequest> Items
    );

    [HttpPost("generate-invoice")]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest req, CancellationToken ct)
    {
        if (req.Items == null || req.Items.Count == 0)
        {
            return BadRequest(new { success = false, message = "At least one grade item is required." });
        }

        // 1. Get or Create Customer
        Customer customer;
        if (req.CustomerId.HasValue && req.CustomerId.Value > 0)
        {
            customer = await _context.Customers.FindAsync(new object[] { req.CustomerId.Value }, ct)
                       ?? throw new Exception("Customer not found");
        }
        else
        {
            var address = string.IsNullOrWhiteSpace(req.ShippingAddress) ? "Direct Counter Sale" : req.ShippingAddress.Trim();
            customer = new Customer
            {
                UserId = null,
                CustomerType = "WHOLESALE_SHOP",
                BusinessName = req.BusinessName?.Trim(),
                ContactPerson = string.IsNullOrWhiteSpace(req.ContactPerson) ? req.BusinessName?.Trim() ?? "Walk-in" : req.ContactPerson.Trim(),
                Phone = req.Phone?.Trim() ?? "",
                GstNumber = req.GstNumber?.Trim(),
                BillingAddress = address,
                ShippingAddress = address,
                City = "Mandi Area",
                State = string.IsNullOrWhiteSpace(req.DestinationState) ? "Andhra Pradesh" : req.DestinationState.Trim(),
                PostalCode = "532222",
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(ct);
        }

        // 2. Calculate Line Items & SubTotal (Each Tin = 10 Kg)
        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var it in req.Items)
        {
            decimal totalWeightKg = it.QuantityTins * 10m;
            decimal lineTotal = totalWeightKg * it.UnitPricePerKg;
            subTotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                GradeId = it.GradeId,
                Quantity = it.QuantityTins,
                WeightPerUnitKg = 10m,
                TotalWeightKg = totalWeightKg,
                UnitPrice = it.UnitPricePerKg,
                LineTotal = lineTotal
            });

            // 3. Deduct from FinishedStock
            var stock = await _context.FinishedStocks.FirstOrDefaultAsync(s => s.GradeId == it.GradeId, ct);
            if (stock != null)
            {
                stock.TotalWeightInKg = Math.Max(0, stock.TotalWeightInKg - totalWeightKg);
                stock.TotalQuantityCount = (int)Math.Floor(stock.TotalWeightInKg / 10m);
                stock.LastAuditedAt = DateTime.UtcNow;
            }
        }

        // 4. GST Calculation (Standard 5% on Cashews HSN 0801)
        bool isInterstate = !req.DestinationState.Equals("Andhra Pradesh", StringComparison.OrdinalIgnoreCase);
        decimal gstRate = 0.05m;
        decimal taxAmount = Math.Round(subTotal * gstRate, 2);
        decimal grandTotal = subTotal + taxAmount;

        string orderNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            GrandTotal = grandTotal,
            PaymentStatus = "PAID",
            OrderStatus = "DISPATCHED",
            OrderedVia = "ERP_BILLING",
            OrderItems = orderItems,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        // 5. Generate E-Way Bill
        string ewayBillNumber = $"EWB{DateTime.UtcNow:yyMMdd}{Random.Shared.Next(100000, 999999)}";
        var ewayBill = new EwayBill
        {
            OrderId = order.Id,
            EwayBillNumber = ewayBillNumber,
            VehicleNumber = req.VehicleNumber.ToUpper().Trim(),
            TransporterName = req.TransporterName?.Trim() ?? "Direct Factory Dispatch",
            ApproxDistanceKm = req.ApproxDistanceKm <= 0 ? 100 : req.ApproxDistanceKm,
            GeneratedDate = DateTime.UtcNow,
            ValidUntilDate = DateTime.UtcNow.AddDays(Math.Max(1, req.ApproxDistanceKm / 200)),
            CreatedAt = DateTime.UtcNow
        };

        _context.EwayBills.Add(ewayBill);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            success = true,
            message = "GST Tax Invoice & E-Way Bill generated successfully.",
            data = new
            {
                order.Id,
                order.OrderNumber,
                ewayBill.EwayBillNumber,
                CustomerName = customer.BusinessName ?? customer.ContactPerson,
                customer.GstNumber,
                req.VehicleNumber,
                SubTotal = subTotal,
                TaxType = isInterstate ? "IGST (5%)" : "CGST (2.5%) + SGST (2.5%)",
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                ItemsCount = orderItems.Count
            }
        });
    }

    [HttpGet("recent-invoices")]
    public async Task<IActionResult> GetRecentInvoices(CancellationToken ct)
    {
        var invoices = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .Take(20)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                CustomerName = o.Customer.BusinessName ?? o.Customer.ContactPerson,
                CustomerPhone = o.Customer.Phone,
                CustomerState = o.Customer.State,
                o.SubTotal,
                o.TaxAmount,
                o.GrandTotal,
                o.PaymentStatus,
                o.OrderStatus,
                Date = o.OrderDate.ToString("yyyy-MM-dd"),
                Time = o.CreatedAt.ToString("hh:mm tt")
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = invoices });
    }
}