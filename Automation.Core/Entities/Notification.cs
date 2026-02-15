namespace Automation.Core.Entities;

/// <summary>
/// اعلان کاربر
/// </summary>
public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "GENERAL";
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string Priority { get; set; } = "LOW";
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
