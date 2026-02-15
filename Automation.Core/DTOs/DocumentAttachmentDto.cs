namespace Automation.Core.DTOs;

/// <summary>
/// DTO برای پیوست
/// </summary>
public class DocumentAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string AttachmentType { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
}




