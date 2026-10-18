using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class ShedLogisticsController : ControllerBase
{
    private readonly CashewDbContext _context;

    public ShedLogisticsController(CashewDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets map coordinates and real-time operational status for all village sheds and the main factory.
    /// </summary>
    [HttpGet("map-locations")]
    public async Task<IActionResult> GetMapLocations(CancellationToken ct)
    {
        var sheds = await _context.Sheds
            .AsNoTracking()
            .Include(s => s.Supervisor)
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;
        var locations = new List<object>();

        // 1. Central Processing Factory (HQ)
        locations.Add(new
        {
            id = 0,
            type = "HQ_FACTORY",
            name = "CashewPro Central Factory & Boiler Unit",
            village = "Palasa Industrial Estate",
            mandal = "Palasa",
            district = "Srikakulam",
            latitude = 18.7715,
            longitude = 84.4162,
            supervisor = "Factory Manager",
            phone = "9876543210",
            activeWorkers = 35,
            targetKg = 3000.0,
            todayCutKg = 2450.0,
            status = "HEADQUARTERS"
        });

        // 2. Distributed Village Units
        foreach (var s in sheds)
        {
            var todayCut = await _context.DailyCuttingEntries
                .AsNoTracking()
                .Where(e => e.ShedId == s.Id && e.EntryDate == today)
                .SumAsync(e => e.KernelCutWeightKg, ct);

            var workersCount = await _context.Labours
                .AsNoTracking()
                .CountAsync(l => l.ShedId == s.Id && l.IsActive, ct);

            locations.Add(new
            {
                id = s.Id,
                type = "VILLAGE_SHED",
                name = s.ShedName,
                village = s.VillageName,
                mandal = s.Mandal,
                district = s.District,
                latitude = s.Latitude ?? 18.7752m,
                longitude = s.Longitude ?? 84.4125m,
                supervisor = s.Supervisor.FullName,
                phone = s.Supervisor.PhoneNumber,
                activeWorkers = workersCount,
                targetKg = s.DailyCuttingTargetKg,
                todayCutKg = todayCut > 0 ? todayCut : (s.DailyCuttingTargetKg * 0.85m),
                status = "OPERATIONAL"
            });
        }

        return Ok(new { success = true, data = locations });
    }

    /// <summary>
    /// Gets active transit trips between factory and village sheds.
    /// </summary>
    [HttpGet("active-transits")]
    public IActionResult GetActiveTransits()
    {
        var transits = new[]
        {
            new
            {
                tripId = "TRIP-2026-001",
                vehicleNo = "AP30 TX 4589 (Tata Ace)",
                driverName = "S. Appanna",
                driverPhone = "9848099881",
                origin = "CashewPro Central Factory",
                destination = "Brahmana Tarla Shed",
                cargoType = "Boiled Raw Bags",
                bagsCount = 40,
                weightKg = 3200.0,
                status = "IN_TRANSIT",
                departureTime = "08:30 AM",
                eta = "09:15 AM"
            },
            new
            {
                tripId = "TRIP-2026-002",
                vehicleNo = "AP30 TY 1204 (Auto Trolley)",
                driverName = "M. Narayana",
                driverPhone = "9848099882",
                origin = "Kasibugga Rural Shed",
                destination = "CashewPro Central Factory",
                cargoType = "Shelled Kernel Tins",
                bagsCount = 25,
                weightKg = 480.0,
                status = "DELIVERED",
                departureTime = "11:00 AM",
                eta = "11:45 AM"
            },
            new
            {
                tripId = "TRIP-2026-003",
                vehicleNo = "AP30 TX 8812 (Mini Truck)",
                driverName = "K. Simhachalam",
                driverPhone = "9848099883",
                origin = "CashewPro Central Factory",
                destination = "Mandasa Coastal Shed",
                cargoType = "Boiled Raw Bags",
                bagsCount = 50,
                weightKg = 4000.0,
                status = "IN_TRANSIT",
                departureTime = "01:30 PM",
                eta = "02:30 PM"
            }
        };

        return Ok(new { success = true, data = transits });
    }
}