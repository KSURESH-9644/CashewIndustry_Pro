using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class User : BaseEntity
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "te"; // 'en', 'te', 'hi'
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}