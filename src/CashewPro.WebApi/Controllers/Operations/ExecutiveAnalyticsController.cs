using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class ExecutiveAnalyticsController : ControllerBase
{
    private readonly CashewDbContext _context;

    public ExecutiveAnalyticsController(CashewDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Delivers an aggregated P&L, inventory valuation, and liability overview for executive directors.
    /// </summary>
    [HttpGet("financial-overview")]
    public async Task<IActionResult> GetFinancialOverview(CancellationToken ct)
    {
        // 1. Total Raw Seeds Procurement Expenditure
        var totalProcurementSpend = await _context.RcnProcurements
            .AsNoTracking()
            .SumAsync(p => p.TotalProcurementCost, ct);

        var totalRawKgProcured = await _context.RcnProcurements
            .AsNoTracking()
            .SumAsync(p => p.NetWeightKg, ct);

        // 2. Total Labour Shelling Wages Paid & Pending
        var totalLabourWagesEarned = await _context.DailyCuttingEntries
            .AsNoTracking()
            .SumAsync(e => e.TotalEarnings, ct);

        var totalPendingLabourBalances = await _context.Labours
            .AsNoTracking()
            .Where(l => l.IsActive)
            .SumAsync(l => l.CurrentBalanceAmount, ct);

        // 3. Realized B2B Sales Revenue
        var totalB2bSalesRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "PAID")
            .SumAsync(o => o.GrandTotal, ct);

        var totalTaxCollected = await _context.Orders
            .AsNoTracking()
            .SumAsync(o => o.TaxAmount, ct);

        // 4. Warehouse Ground Stock Current Market Valuation
        var stockItems = await _context.FinishedStocks
            .AsNoTracking()
            .Include(s => s.Grade)
            .ToListAsync(ct);

        decimal currentInventoryValuation = stockItems
            .Sum(s => s.TotalWeightInKg * s.Grade.CurrentWholesalePricePerKg);

        decimal totalStockKg = stockItems.Sum(s => s.TotalWeightInKg);

        // 5. Total Operating Costs & Estimated Gross Margin
        decimal totalDirectExpenses = totalProcurementSpend + totalLabourWagesEarned;
        decimal netFactoryWorth = currentInventoryValuation + totalB2bSalesRevenue - totalDirectExpenses;
        decimal grossProfit = totalB2bSalesRevenue - (totalLabourWagesEarned * 0.4m); // dynamic margin preview

        return Ok(new
        {
            success = true,
            data = new
            {
                procurement = new
                {
                    totalSpend = totalProcurementSpend,
                    totalKg = totalRawKgProcured,
                    averageRatePerQuintal = totalRawKgProcured > 0
                        ? Math.Round((totalProcurementSpend / (totalRawKgProcured / 100m)), 2)
                        : 12500m
                },
                labourWages = new
                {
                    totalEarned = totalLabourWagesEarned,
                    totalPendingBalance = totalPendingLabourBalances
                },
                sales = new
                {
                    totalRevenue = totalB2bSalesRevenue,
                    taxCollected = totalTaxCollected,
                    grossProfit
                },
                inventory = new
                {
                    totalWeightKg = totalStockKg,
                    marketValuation = Math.Round(currentInventoryValuation, 2)
                },
                financialHealth = new
                {
                    netFactoryWorth = Math.Round(netFactoryWorth, 2),
                    totalPendingLiabilities = totalPendingLabourBalances
                }
            }
        });
    }

    /// <summary>
    /// Comparative yield, efficiency, and shell wastage ratio across distributed village units.
    /// </summary>
    [HttpGet("shed-efficiency-matrix")]
    public async Task<IActionResult> GetShedEfficiencyMatrix(CancellationToken ct)
    {
        var sheds = await _context.Sheds
            .AsNoTracking()
            .Include(s => s.Supervisor)
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        var result = new List<object>();

        foreach (var shed in sheds)
        {
            var entries = await _context.DailyCuttingEntries
                .AsNoTracking()
                .Where(e => e.ShedId == shed.Id)
                .ToListAsync(ct);

            var activeWorkersCount = await _context.Labours
                .AsNoTracking()
                .CountAsync(l => l.ShedId == shed.Id && l.IsActive, ct);

            decimal totalRawIssued = entries.Sum(e => e.RawSeedsIssuedKg);
            decimal totalKernelCut = entries.Sum(e => e.KernelCutWeightKg);
            decimal totalWastage = entries.Sum(e => e.UncutOrWasteWeightKg);

            decimal yieldRatio = totalRawIssued > 0
                ? Math.Round((totalKernelCut / totalRawIssued) * 100m, 2)
                : 23.5m;

            decimal targetAchievedPct = shed.DailyCuttingTargetKg > 0
                ? Math.Round((totalKernelCut / shed.DailyCuttingTargetKg) * 100m, 2)
                : 0m;

            result.Add(new
            {
                shed.Id,
                shed.ShedCode,
                shed.ShedName,
                shed.VillageName,
                shed.Mandal,
                SupervisorName = shed.Supervisor.FullName,
                ActiveWorkers = activeWorkersCount,
                DailyTargetKg = shed.DailyCuttingTargetKg,
                TotalCutKg = totalKernelCut,
                TotalWastageKg = totalWastage,
                YieldRatio = yieldRatio,
                TargetAchievedPct = targetAchievedPct,
                Status = yieldRatio >= 23.0m ? "HIGH_EFFICIENCY" : (yieldRatio >= 20.5m ? "NORMAL" : "ATTENTION_REQUIRED")
            });
        }

        return Ok(new { success = true, data = result });
    }
}