using CashewPro.Domain.Entities;
using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class ProcurementController : ControllerBase
{
    private readonly CashewDbContext _context;

    public ProcurementController(CashewDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all registered farmers and external vendors.
    /// </summary>
    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers(CancellationToken ct)
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.FullName)
            .Select(s => new
            {
                s.Id,
                s.FullName,
                s.PhoneNumber,
                s.SupplierType,
                s.Village,
                s.District,
                s.BankAccountNumber,
                s.IfscCode
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = suppliers });
    }

    public record CreateSupplierRequest(
        string FullName,
        string PhoneNumber,
        string SupplierType,
        string? Village,
        string? District,
        string? State,
        string? BankAccountNumber,
        string? IfscCode
    );

    /// <summary>
    /// Registers a new farmer or Mandi supplier.
    /// </summary>
    [HttpPost("suppliers")]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.PhoneNumber))
        {
            return BadRequest(new { success = false, message = "Name and phone number are required." });
        }

        var supplier = new Supplier
        {
            FullName = req.FullName.Trim(),
            PhoneNumber = req.PhoneNumber.Trim(),
            SupplierType = req.SupplierType,
            Village = req.Village,
            District = req.District ?? "Srikakulam",
            State = req.State ?? "Andhra Pradesh",
            BankAccountNumber = req.BankAccountNumber,
            IfscCode = req.IfscCode,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(ct);

        return Ok(new { success = true, message = "Farmer registered successfully.", data = supplier });
    }

    public record RecordProcurementRequest(
        long SupplierId,
        string OriginMandi,
        string VehicleNumber,
        string? DriverName,
        string? DriverPhone,
        int BagsCount,
        decimal GrossWeightKg,
        decimal TareWeightKg,
        decimal? MoisturePercentage,
        int? NutCountPerKg,
        decimal? OutturnLbs,
        decimal RatePerQuintal
    );

    /// <summary>
    /// Records a raw seed weighbridge inward entry with auto net-weight & total cost calculation.
    /// </summary>
    [HttpPost("record-inward")]
    public async Task<IActionResult> RecordInward([FromBody] RecordProcurementRequest req, CancellationToken ct)
    {
        if (req.GrossWeightKg <= req.TareWeightKg)
        {
            return BadRequest(new { success = false, message = "Gross weight must be strictly greater than Tare (empty vehicle) weight." });
        }

        var supplier = await _context.Suppliers.FindAsync(new object[] { req.SupplierId }, ct);
        if (supplier == null)
        {
            return NotFound(new { success = false, message = "Selected farmer/supplier does not exist." });
        }

        decimal netWeightKg = Math.Round(req.GrossWeightKg - req.TareWeightKg, 3);
        // Cost: 1 Quintal = 100 Kg
        decimal totalCost = Math.Round((netWeightKg / 100m) * req.RatePerQuintal, 2);

        string lotNumber = $"LOT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var procurement = new RcnProcurement
        {
            LotNumber = lotNumber,
            SupplierId = req.SupplierId,
            OriginMandi = string.IsNullOrWhiteSpace(req.OriginMandi) ? "Palasa Mandi" : req.OriginMandi,
            VehicleNumber = req.VehicleNumber.ToUpper().Trim(),
            DriverName = req.DriverName,
            DriverPhone = req.DriverPhone,
            BagsCount = req.BagsCount,
            GrossWeightKg = req.GrossWeightKg,
            TareWeightKg = req.TareWeightKg,
            NetWeightKg = netWeightKg,
            MoisturePercentage = req.MoisturePercentage,
            NutCountPerKg = req.NutCountPerKg,
            OutturnLbs = req.OutturnLbs,
            RatePerQuintal = req.RatePerQuintal,
            TotalProcurementCost = totalCost,
            ProcurementDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        };

        _context.RcnProcurements.Add(procurement);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            success = true,
            message = "Raw seed inward batch registered successfully.",
            data = new
            {
                procurement.Id,
                procurement.LotNumber,
                SupplierName = supplier.FullName,
                procurement.VehicleNumber,
                procurement.BagsCount,
                procurement.NetWeightKg,
                NetWeightQuintals = Math.Round(netWeightKg / 100m, 2),
                procurement.RatePerQuintal,
                procurement.TotalProcurementCost
            }
        });
    }

    /// <summary>
    /// Gets today's or recent inward consignments.
    /// </summary>
    [HttpGet("recent-inwards")]
    public async Task<IActionResult> GetRecentInwards(CancellationToken ct)
    {
        var records = await _context.RcnProcurements
            .AsNoTracking()
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.CreatedAt)
            .Take(25)
            .Select(p => new
            {
                p.Id,
                p.LotNumber,
                SupplierName = p.Supplier.FullName,
                SupplierPhone = p.Supplier.PhoneNumber,
                p.VehicleNumber,
                p.OriginMandi,
                p.BagsCount,
                p.GrossWeightKg,
                p.TareWeightKg,
                p.NetWeightKg,
                p.MoisturePercentage,
                p.OutturnLbs,
                p.RatePerQuintal,
                p.TotalProcurementCost,
                ProcurementDate = p.ProcurementDate.ToString("yyyy-MM-dd"),
                Time = p.CreatedAt.ToString("hh:mm tt")
            })
            .ToListAsync(ct);

        var totalNetKg = records.Sum(r => r.NetWeightKg);
        var totalAmount = records.Sum(r => r.TotalProcurementCost);

        return Ok(new
        {
            success = true,
            totalNetKg,
            totalAmount,
            count = records.Count,
            data = records
        });
    }
}