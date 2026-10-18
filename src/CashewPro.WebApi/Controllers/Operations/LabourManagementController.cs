using CashewPro.Domain.Entities;
using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers.Operations;

[ApiController]
[Route("api/v1/erp/[controller]")]
public class LabourManagementController : ControllerBase
{
    private readonly CashewDbContext _context;

    public LabourManagementController(CashewDbContext context)
    {
        _context = context;
    }

    [HttpGet("labours")]
    public async Task<IActionResult> GetAllLabours([FromQuery] int? shedId, CancellationToken ct)
    {
        var query = _context.Labours
            .AsNoTracking()
            .Include(l => l.Shed)
            .Where(l => l.IsActive);

        if (shedId.HasValue && shedId.Value > 0)
        {
            query = query.Where(l => l.ShedId == shedId.Value);
        }

        var labours = await query
            .OrderByDescending(l => l.CurrentBalanceAmount)
            .Select(l => new
            {
                l.Id,
                l.LabourCode,
                l.FullName,
                l.MobileNumber,
                l.ShedId,
                ShedName = l.Shed.ShedName,
                l.WorkType,
                l.WageBasis,
                Rate = l.PieceRatePerKg,
                l.CurrentBalanceAmount,
                l.PreferredLanguage,
                TotalCutKg = l.CuttingEntries.Sum(c => c.KernelCutWeightKg)
            })
            .ToListAsync(ct);

        var totalPendingWages = labours.Sum(l => l.CurrentBalanceAmount);
        var totalWorkforceCount = labours.Count;

        return Ok(new
        {
            success = true,
            totalWorkforceCount,
            totalPendingWages,
            data = labours
        });
    }

    public record CreateLabourRequest(
        string FullName,
        string MobileNumber,
        int ShedId,
        string WorkType,
        string WageBasis,
        decimal Rate,
        string PreferredLanguage,
        string? AadhaarNumber
    );

    [HttpPost("register")]
    public async Task<IActionResult> RegisterLabour([FromBody] CreateLabourRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.MobileNumber))
        {
            return BadRequest(new { success = false, message = "Name and mobile number are mandatory." });
        }

        string code = $"LAB-{Random.Shared.Next(1000, 9999)}";

        var labour = new Labour
        {
            LabourCode = code,
            FullName = req.FullName.Trim(),
            MobileNumber = req.MobileNumber.Trim(),
            ShedId = req.ShedId > 0 ? req.ShedId : 1,
            WorkType = req.WorkType,
            WageBasis = req.WageBasis,
            PieceRatePerKg = req.Rate,
            PreferredLanguage = string.IsNullOrWhiteSpace(req.PreferredLanguage) ? "te" : req.PreferredLanguage,
            AadhaarNumber = req.AadhaarNumber,
            CurrentBalanceAmount = 0.00m,
            IsActive = true,
            JoinedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Labours.Add(labour);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            success = true,
            message = $"{labour.FullName} ({labour.WorkType}) successfully registered in database.",
            data = labour
        });
    }

    public record SettleWagePaymentRequest(
        long LabourId,
        decimal Amount,
        string PaymentMode, // CASH, UPI, BANK_TRANSFER
        string? TransactionRef,
        string? Notes,
        long ProcessedByUserId
    );

    [HttpPost("settle-payment")]
    public async Task<IActionResult> SettlePayment([FromBody] SettleWagePaymentRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
        {
            return BadRequest(new { success = false, message = "Payment amount must be greater than 0." });
        }

        var labour = await _context.Labours
            .Include(l => l.Shed)
            .FirstOrDefaultAsync(l => l.Id == req.LabourId, ct);

        if (labour == null)
        {
            return NotFound(new { success = false, message = "Labour not found." });
        }

        try
        {
            decimal previousBalance = labour.CurrentBalanceAmount;
            labour.CurrentBalanceAmount = Math.Max(0, labour.CurrentBalanceAmount - req.Amount);
            labour.UpdatedAt = DateTime.UtcNow;

            // ProcessedByUser fallback verification
            var activeUserId = req.ProcessedByUserId > 0 ? req.ProcessedByUserId : 1;
            var userExists = await _context.Users.AnyAsync(u => u.Id == activeUserId, ct);
            if (!userExists)
            {
                var firstUser = await _context.Users.Select(u => u.Id).FirstOrDefaultAsync(ct);
                activeUserId = firstUser > 0 ? firstUser : 1;
            }

            var payment = new LabourPayment
            {
                LabourId = labour.Id,
                ShedId = labour.ShedId,
                PaymentDate = DateTime.UtcNow,
                AmountPaid = req.Amount,
                PaymentMode = string.IsNullOrWhiteSpace(req.PaymentMode) ? "CASH" : req.PaymentMode.Trim().ToUpper(),
                TransactionRefNumber = req.TransactionRef,
                ProcessedByUserId = activeUserId,
                Notes = req.Notes ?? "Wage settlement",
                CreatedAt = DateTime.UtcNow
            };

            _context.LabourPayments.Add(payment);
            await _context.SaveChangesAsync(ct);

            // Formatted WhatsApp Receipt
            string lang = !string.IsNullOrWhiteSpace(labour.PreferredLanguage) ? labour.PreferredLanguage.ToLower() : "te";
            string receiptMsg = lang switch
            {
                "te" => $"నమస్తే {labour.FullName},\n" +
                        $"CashewPro {labour.Shed.ShedName} వేతన చెల్లింపు రసీదు:\n" +
                        $"• చెల్లించిన మొత్తం: రూ. {req.Amount:0.00}\n" +
                        $"• పేమెంట్ మోడ్: {payment.PaymentMode}\n" +
                        $"• మునుపటి బకాయి: రూ. {previousBalance:0.00}\n" +
                        $"• ప్రస్తుత నికర బ్యాలెన్స్: రూ. {labour.CurrentBalanceAmount:0.00}\n" +
                        $"ధన్యవాదాలు!",
                "hi" => $"नमस्ते {labour.FullName},\n" +
                        $"CashewPro मजदूरी भुगतान रसीद:\n" +
                        $"• भुगतान राशि: रु. {req.Amount:0.00}\n" +
                        $"• माध्यम: {payment.PaymentMode}\n" +
                        $"• शेष बकाया: रु. {labour.CurrentBalanceAmount:0.00}\n" +
                        $"धन्यवाद!",
                _ => $"Hello {labour.FullName},\n" +
                     $"CashewPro Wage Settlement Receipt:\n" +
                     $"• Amount Paid: Rs. {req.Amount:0.00}\n" +
                     $"• Mode: {payment.PaymentMode}\n" +
                     $"• Remaining Balance: Rs. {labour.CurrentBalanceAmount:0.00}\n" +
                     $"Thank you!"
            };

            return Ok(new
            {
                success = true,
                message = $"Payment of ₹{req.Amount} committed to ledger.",
                data = new
                {
                    paymentId = payment.Id,
                    labourId = labour.Id,
                    labourName = labour.FullName,
                    amountPaid = req.Amount,
                    previousBalance,
                    currentBalance = labour.CurrentBalanceAmount,
                    whatsAppUrl = $"https://wa.me/91{labour.MobileNumber}?text={Uri.EscapeDataString(receiptMsg)}"
                }
            });
        }
        catch (DbUpdateException ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { success = false, message = "Database Error: " + innerMsg });
        }
    }
}