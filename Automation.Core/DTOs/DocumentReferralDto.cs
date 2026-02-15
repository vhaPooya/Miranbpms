namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای ارجاع
/// </summary>
public class DocumentReferralDto
{
    public int Id { get; set; }
    public string ActionTypeName { get; set; } = string.Empty;
    public string ReferredToName { get; set; } = string.Empty;
    public DateTime ReferredDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}




