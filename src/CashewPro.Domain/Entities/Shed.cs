using CashewPro.Domain.Common;

namespace CashewPro.Domain.Entities;

public class Shed
{
    public int Id { get; set; }
    public string ShedCode { get; set; } = string.Empty;
    public string ShedName { get; set; } = string.Empty;

    public long SupervisorUserId { get; set; }
    public User Supervisor { get; set; } = null!;

    public string VillageName { get; set; } = string.Empty;
    public string Mandal { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public decimal DailyCuttingTargetKg { get; set; } = 500.000m;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Labour> Labours { get; set; } = new List<Labour>();
    public ICollection<DailyCuttingEntry> DailyCuttingEntries { get; set; } = new List<DailyCuttingEntry>();
}