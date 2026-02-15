namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای به‌روزرسانی دبیرخانه
/// </summary>
public class UpdateSecretariatDto
{
    public string SecretariatCode { get; set; } = string.Empty;
    public string SecretariatName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string? EmailAddress { get; set; }
    public string? FaxNumber { get; set; }
    public bool? IsActive { get; set; }
    public string? Description { get; set; }
}





