using CashewPro.Domain.Entities;
using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class ShedOperationsController : ControllerBase
{
    private readonly CashewDbContext _context;

    public ShedOperationsController(CashewDbContext context)
    {
        _context = context;
    }

    [HttpGet("sheds")]
    public async Task<IActionResult> GetSheds(CancellationToken ct)
    {
        var sheds = await _context.Sheds
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.ShedCode,
                s.ShedName,
                s.VillageName,
                s.Mandal,
                s.DailyCuttingTargetKg,
                LabourCount = s.Labours.Count(l => l.IsActive)
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = sheds });
    }

    [HttpGet("sheds/{shedId}/labours")]
    public async Task<IActionResult> GetShedLabours(int shedId, CancellationToken ct)
    {
        var labours = await _context.Labours
            .AsNoTracking()
            .Where(l => l.ShedId == shedId && l.IsActive)
            .OrderBy(l => l.FullName)
            .Select(l => new
            {
                l.Id,
                l.LabourCode,
                l.FullName,
                l.MobileNumber,
                l.PreferredLanguage,
                l.PieceRatePerKg,
                l.CurrentBalanceAmount
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = labours });
    }

    public record SaveOrUpdateCuttingRequest(
        long? EntryId,
        int ShedId,
        long LabourId,
        decimal RawIssuedKg,
        decimal KernelCutKg,
        decimal WasteKg,
        bool SendWhatsApp,
        bool SendSms,
        string? MessageLanguage, // 'te', 'hi', 'en'
        long EnteredByUserId
    );

    /// <summary>
    /// Records or updates daily cutting, recalculates monthly totals and dispatches WhatsApp / SMS.
    /// </summary>
    [HttpPost("save-cutting")]
    public async Task<IActionResult> SaveCutting([FromBody] SaveOrUpdateCuttingRequest request, CancellationToken ct)
    {
        if (request.KernelCutKg <= 0)
        {
            return BadRequest(new { success = false, message = "Kernel cut weight must be greater than 0 kg." });
        }

        var labour = await _context.Labours.FirstOrDefaultAsync(l => l.Id == request.LabourId, ct);
        if (labour == null)
        {
            return NotFound(new { success = false, message = "Labour record not found." });
        }

        var rate = labour.PieceRatePerKg;
        var totalEarnings = Math.Round(request.KernelCutKg * rate, 2);
        DailyCuttingEntry? entry;
        bool isUpdate = false;

        if (request.EntryId.HasValue && request.EntryId.Value > 0)
        {
            entry = await _context.DailyCuttingEntries.FirstOrDefaultAsync(e => e.Id == request.EntryId.Value, ct);
            if (entry == null) return NotFound(new { success = false, message = "Entry to update not found." });

            isUpdate = true;
            // Rollback previous earning from balance, then add updated
            labour.CurrentBalanceAmount = labour.CurrentBalanceAmount - entry.TotalEarnings + totalEarnings;

            entry.RawSeedsIssuedKg = request.RawIssuedKg;
            entry.KernelCutWeightKg = request.KernelCutKg;
            entry.UncutOrWasteWeightKg = request.WasteKg;
            entry.CalculatedRatePerKg = rate;
            entry.TotalEarnings = totalEarnings;
            entry.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            entry = new DailyCuttingEntry
            {
                BoilingBatchId = 1,
                ShedId = request.ShedId,
                LabourId = request.LabourId,
                EntryDate = DateTime.UtcNow.Date,
                RawSeedsIssuedKg = request.RawIssuedKg,
                KernelCutWeightKg = request.KernelCutKg,
                UncutOrWasteWeightKg = request.WasteKg,
                CalculatedRatePerKg = rate,
                TotalEarnings = totalEarnings,
                EnteredByUserId = request.EnteredByUserId,
                IsWhatsAppNotified = request.SendWhatsApp,
                CreatedAt = DateTime.UtcNow
            };

            labour.CurrentBalanceAmount += totalEarnings;
            _context.DailyCuttingEntries.Add(entry);
        }

        await _context.SaveChangesAsync(ct);

        // Calculate this month's stats (1st of month to today)
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var monthEntries = await _context.DailyCuttingEntries
            .AsNoTracking()
            .Where(e => e.LabourId == request.LabourId && e.EntryDate >= firstDayOfMonth)
            .ToListAsync(ct);

        var monthTotalKg = monthEntries.Sum(e => e.KernelCutWeightKg);
        var monthTotalEarnings = monthEntries.Sum(e => e.TotalEarnings);

        // Language resolution: request explicit selection > labour profile preference > 'te'
        string lang = !string.IsNullOrWhiteSpace(request.MessageLanguage)
            ? request.MessageLanguage.ToLower()
            : (!string.IsNullOrWhiteSpace(labour.PreferredLanguage) ? labour.PreferredLanguage.ToLower() : "te");

        string statusPrefix = isUpdate ? "(Updated / సవరణ)" : "";
        string messageText = lang switch
        {
            "te" => $"నమస్తే {labour.FullName} {statusPrefix},\n" +
                    $"CashewPro Brahmana Tarla Shed:\n" +
                    $"• ఈరోజు కట్టింగ్: {request.KernelCutKg:0.000} Kg (రూ. {totalEarnings})\n" +
                    $"• ఈ నెల మొత్తం: {monthTotalKg:0.000} Kg (రూ. {monthTotalEarnings})\n" +
                    $"• మొత్తం బకాయి: రూ. {labour.CurrentBalanceAmount}\n" +
                    $"ధన్యవాదాలు!",
            "hi" => $"नमस्ते {labour.FullName} {statusPrefix},\n" +
                    $"CashewPro शेड रिपोर्ट:\n" +
                    $"• आज की कटाई: {request.KernelCutKg:0.000} Kg (रु. {totalEarnings})\n" +
                    $"• इस महीने कुल: {monthTotalKg:0.000} Kg (रु. {monthTotalEarnings})\n" +
                    $"• कुल बकाया राशि: रु. {labour.CurrentBalanceAmount}\n" +
                    $"धन्यवाद!",
            _ => $"Hello {labour.FullName} {statusPrefix},\n" +
                 $"CashewPro Shed Update:\n" +
                 $"• Today Cut: {request.KernelCutKg:0.000} Kg (Rs. {totalEarnings})\n" +
                 $"• Month Total: {monthTotalKg:0.000} Kg (Rs. {monthTotalEarnings})\n" +
                 $"• Total Balance: Rs. {labour.CurrentBalanceAmount}"
        };

        return Ok(new
        {
            success = true,
            isUpdate,
            message = isUpdate ? "Cutting entry updated and notification dispatched." : "Cutting entry recorded and notification dispatched.",
            data = new
            {
                entryId = entry.Id,
                cutWeightKg = request.KernelCutKg,
                totalEarnings,
                monthTotalKg,
                monthTotalEarnings,
                labourBalance = labour.CurrentBalanceAmount,
                channels = new
                {
                    language = lang,
                    whatsAppSent = request.SendWhatsApp,
                    smsSent = request.SendSms,
                    mobileNumber = labour.MobileNumber,
                    message = messageText,
                    whatsAppUrl = $"https://wa.me/91{labour.MobileNumber}?text={Uri.EscapeDataString(messageText)}"
                }
            }
        });
    }

    /// <summary>
    /// Deletes a daily cutting entry and rolls back the earned amount from the labour's ledger balance.
    /// </summary>
    [HttpDelete("entries/{id}")]
    public async Task<IActionResult> DeleteEntry(long id, CancellationToken ct)
    {
        var entry = await _context.DailyCuttingEntries.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entry == null)
        {
            return NotFound(new { success = false, message = "Cutting entry not found." });
        }

        var labour = await _context.Labours.FirstOrDefaultAsync(l => l.Id == entry.LabourId, ct);
        if (labour != null)
        {
            labour.CurrentBalanceAmount -= entry.TotalEarnings;
            if (labour.CurrentBalanceAmount < 0) labour.CurrentBalanceAmount = 0;
        }

        _context.DailyCuttingEntries.Remove(entry);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            success = true,
            message = "Entry deleted successfully and labour balance adjusted.",
            rolledBackAmount = entry.TotalEarnings
        });
    }

    [HttpGet("sheds/{shedId}/today-entries")]
    public async Task<IActionResult> GetTodayShedEntries(int shedId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var entries = await _context.DailyCuttingEntries
            .AsNoTracking()
            .Where(e => e.ShedId == shedId && e.EntryDate == today)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.LabourId,
                LabourName = e.Labour.FullName,
                LabourCode = e.Labour.LabourCode,
                e.RawSeedsIssuedKg,
                e.KernelCutWeightKg,
                e.UncutOrWasteWeightKg,
                e.TotalEarnings,
                e.CalculatedRatePerKg,
                Time = e.CreatedAt.ToString("hh:mm tt")
            })
            .ToListAsync(ct);

        var totalCutKg = entries.Sum(e => e.KernelCutWeightKg);
        var totalAmount = entries.Sum(e => e.TotalEarnings);

        return Ok(new
        {
            success = true,
            totalCutKg,
            totalAmount,
            count = entries.Count,
            data = entries
        });
    }
}