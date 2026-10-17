using CashewPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashewPro.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly CashewDbContext _context;

    public AuthController(CashewDbContext context)
    {
        _context = context;
    }

    public record LoginRequest(string Identifier, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, message = "Please enter valid mobile/email and password." });
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => (u.PhoneNumber == request.Identifier || u.Email == request.Identifier) && u.IsActive, ct);

        // Demo purpose verify - In production use BCrypt / Argon2
        if (user == null || user.PasswordHash != request.Password)
        {
            return Unauthorized(new { success = false, message = "Invalid phone/email or password." });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                token = "mock_jwt_token_" + Guid.NewGuid(),
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.PhoneNumber,
                    user.Email,
                    Role = user.Role.RoleCode,
                    RoleName = user.Role.RoleName,
                    user.PreferredLanguage
                }
            }
        });
    }
}