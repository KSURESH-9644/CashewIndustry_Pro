using CashewPro.Domain.Entities;
using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class GradingStockController : ControllerBase
{
    private readonly CashewDbContext _context;

    public GradingStockController(CashewDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets active boiling batches available for grading.
    /// </summary>
    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches(CancellationToken ct)
    {
        var batches = await _context.BoilingBatches
            .AsNoTracking()
            .Select(b => new
            {
                b.Id,
                b.BatchNumber,
                b.RawWeightInputKg,
                b.RawBagsUsed,
                b.Status,
                BoilingDate = b.BoilingDate.ToString("yyyy-MM-dd")
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = batches });
    }

    /// <summary>
    /// Gets current warehouse ground stock breakdown per grade.
    /// </summary>
    [HttpGet("stock-matrix")]
    public async Task<IActionResult> GetStockMatrix(CancellationToken ct)
    {
        var stock = await _context.FinishedStocks
            .AsNoTracking()
            .Include(s => s.Grade)
            .Select(s => new
            {
                s.Id,
                s.GradeId,
                GradeCode = s.Grade.GradeCode,
                GradeName = s.Grade.GradeName,
                Category = s.Grade.GradeCategory,
                RetailPrice = s.Grade.CurrentRetailPricePerKg,
                WholesalePrice = s.Grade.CurrentWholesalePricePerKg,
                s.PackageType,
                s.TotalQuantityCount,
                s.TotalWeightInKg,
                EstimatedValuation = Math.Round(s.TotalWeightInKg * s.Grade.CurrentWholesalePricePerKg, 2)
            })
            .ToListAsync(ct);

        var totalWeight = stock.Sum(s => s.TotalWeightInKg);
        var totalValuation = stock.Sum(s => s.EstimatedValuation);

        return Ok(new
        {
            success = true,
            totalWeightKg = totalWeight,
            totalValuation = totalValuation,
            data = stock
        });
    }

    public record GradeOutputItem(int GradeId, decimal WeightKg);

    public record RecordGradingBatchRequest(
        long BoilingBatchId,
        decimal RawNutInputWeightKg,
        List<GradeOutputItem> GradedOutputs,
        string? Notes
    );

    /// <summary>
    /// Records grade sorting output from a batch, calculates kernel recovery yield %, and commits to finished stock.
    /// </summary>
    [HttpPost("record-grading")]
    public async Task<IActionResult> RecordGrading([FromBody] RecordGradingBatchRequest req, CancellationToken ct)
    {
        if (req.RawNutInputWeightKg <= 0)
        {
            return BadRequest(new { success = false, message = "Raw nut input weight must be greater than 0 kg." });
        }

        if (req.GradedOutputs == null || req.GradedOutputs.Count == 0)
        {
            return BadRequest(new { success = false, message = "At least one grade weight output is required." });
        }

        decimal totalKernelWeightKg = req.GradedOutputs.Sum(g => g.WeightKg);
        if (totalKernelWeightKg <= 0)
        {
            return BadRequest(new { success = false, message = "Total graded kernel weight cannot be zero." });
        }

        // Standard Yield Formula: (Kernel Output Kg / Raw Nut Input Kg) * 100
        decimal recoveryYieldPercentage = Math.Round((totalKernelWeightKg / req.RawNutInputWeightKg) * 100m, 2);

        // Update Finished Stock Ledger
        foreach (var item in req.GradedOutputs)
        {
            if (item.WeightKg <= 0) continue;

            var stock = await _context.FinishedStocks.FirstOrDefaultAsync(s => s.GradeId == item.GradeId, ct);
            if (stock != null)
            {
                stock.TotalWeightInKg += item.WeightKg;
                stock.TotalQuantityCount = (int)Math.Floor(stock.TotalWeightInKg / 10m); // Standard 10kg export tin packaging
                stock.LastAuditedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            success = true,
            message = "Grading batch processed and finished stock updated successfully.",
            data = new
            {
                rawInputKg = req.RawNutInputWeightKg,
                totalKernelOutputKg = totalKernelWeightKg,
                recoveryYieldPercentage = recoveryYieldPercentage,
                yieldCategory = recoveryYieldPercentage >= 23.5m ? "OPTIMAL_HIGH" : (recoveryYieldPercentage >= 21.0m ? "NORMAL" : "LOW_RECOVERY_ALERT")
            }
        });
    }
}