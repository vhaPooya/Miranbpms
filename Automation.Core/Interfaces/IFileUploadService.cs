namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس آپلود و مدیریت فایل‌ها
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// آپلود فایل
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType, int documentId, string category = "attachments");
    
    /// <summary>
    /// حذف فایل
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath);
    
    /// <summary>
    /// دریافت مسیر کامل فایل
    /// </summary>
    string GetFilePath(int documentId, string fileName, string category = "attachments");
    
    /// <summary>
    /// بررسی اعتبار فایل (نوع و حجم)
    /// </summary>
    bool ValidateFile(string fileName, long fileSize, out string? errorMessage);
    
    /// <summary>
    /// دریافت URL فایل برای دسترسی از وب
    /// </summary>
    string GetFileUrl(string filePath);
}

/// <summary>
/// نتیجه آپلود فایل
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? ErrorMessage { get; set; }
}




