using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Storefront;

[ApiController]
[Route("api/v1/store/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly CashewDbContext _context;

    public ProductsController(CashewDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Fetches all active cashew grades for the customer storefront showcase.
    /// </summary>
    [HttpGet("grades")]
    public async Task<IActionResult> GetActiveGrades(CancellationToken cancellationToken = default)
    {
        try
        {
            var grades = await _context.ProductGrades
                .AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.CurrentRetailPricePerKg)
                .Select(g => new
                {
                    g.Id,
                    g.GradeCode,
                    g.GradeName,
                    g.GradeCategory,
                    g.StandardDescription,
                    g.CurrentRetailPricePerKg,
                    g.CurrentWholesalePricePerKg,
                    g.MinimumWholesaleQtyKg,
                    g.ImageUrl
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                Success = true,
                Count = grades.Count,
                Data = grades
            });
        }
        catch (OperationCanceledException)
        {
            // Browser connection aborted/reloaded - ignore cleanly
            return StatusCode(499); // Client Closed Request
        }
    }

    /// <summary>
    /// Fetches live market prices for the storefront top ticker ribbon.
    /// </summary>
    [HttpGet("live-ticker")]
    public async Task<IActionResult> GetLiveMarketTicker(CancellationToken cancellationToken = default)
    {
        try
        {
            var ticker = await _context.DailyMarketPrices
                .AsNoTracking()
                .Include(p => p.Grade)
                .OrderByDescending(p => p.PriceDate)
                .Take(10)
                .Select(p => new
                {
                    p.Id,
                    GradeCode = p.Grade.GradeCode,
                    GradeName = p.Grade.GradeName,
                    p.MandiMarketName,
                    p.PricePerKg,
                    p.PriceTrend,
                    p.PriceDate
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                Success = true,
                Count = ticker.Count,
                Data = ticker
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
    }
}