using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس آپلود و مدیریت فایل‌ها
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly string _baseUploadPath;
    
    // انواع فایل مجاز
    private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".txt", ".zip", ".rar" };
    
    // حداکثر حجم فایل (50 مگابایت)
    private readonly long _maxFileSize = 50 * 1024 * 1024;

    public FileUploadService(ILogger<FileUploadService> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        var webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        _baseUploadPath = Path.Combine(Directory.Exists(webRootPath) ? webRootPath : _environment.ContentRootPath, "uploads");
        
        // ایجاد پوشه اصلی در صورت عدم وجود
        if (!Directory.Exists(_baseUploadPath))
        {
            Directory.CreateDirectory(_baseUploadPath);
        }
    }

    public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType, int documentId, string category = "attachments")
    {
        try
        {
            // اعتبارسنجی فایل
            if (!ValidateFile(fileName, fileStream.Length, out var errorMessage))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }

            // ساختار پوشه: /uploads/{Year}/{Month}/{DocumentId}/{category}/
            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month.ToString("00");
            
            var documentFolder = Path.Combine(_baseUploadPath, year.ToString(), month, documentId.ToString(), category);
            
            if (!Directory.Exists(documentFolder))
            {
                Directory.CreateDirectory(documentFolder);
            }

            // نام فایل امن
            var safeFileName = SanitizeFileName(fileName);
            var filePath = Path.Combine(documentFolder, safeFileName);
            
            // ذخیره فایل
            using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamWriter);
            }

            var relativePath = Path.GetRelativePath(_baseUploadPath, filePath).Replace('\\', '/');

            _logger.LogInformation("File uploaded: {FilePath} for document {DocumentId}", relativePath, documentId);

            return new FileUploadResult
            {
                Success = true,
                FilePath = relativePath,
                FileName = safeFileName,
                FileSize = fileStream.Length,
                MimeType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for document {DocumentId}", fileName, documentId);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            return false;
        }
    }

    public string GetFilePath(int documentId, string fileName, string category = "attachments")
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month.ToString("00");
        
        var documentFolder = Path.Combine(_baseUploadPath, year.ToString(), month, documentId.ToString(), category);
        var safeFileName = SanitizeFileName(fileName);
        
        return Path.Combine(documentFolder, safeFileName);
    }

    public bool ValidateFile(string fileName, long fileSize, out string? errorMessage)
    {
        errorMessage = null;

        // بررسی پسوند فایل
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            errorMessage = $"نوع فایل مجاز نیست. انواع مجاز: {string.Join(", ", _allowedExtensions)}";
            return false;
        }

        // بررسی حجم فایل
        if (fileSize > _maxFileSize)
        {
            errorMessage = $"حجم فایل بیش از حد مجاز است. حداکثر حجم: {_maxFileSize / (1024 * 1024)} مگابایت";
            return false;
        }

        if (fileSize == 0)
        {
            errorMessage = "فایل خالی است";
            return false;
        }

        return true;
    }

    public string GetFileUrl(string filePath)
    {
        var relativePath = filePath.Replace('\\', '/');
        if (!relativePath.StartsWith("/"))
        {
            relativePath = "/" + relativePath;
        }
        return $"/uploads{relativePath}";
    }

    private string SanitizeFileName(string fileName)
    {
        // حذف کاراکترهای غیرمجاز
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // حذف فاصله‌های اضافی
        sanitized = Regex.Replace(sanitized, @"\s+", "_");
        
        // محدود کردن طول نام فایل
        var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
        var ext = Path.GetExtension(sanitized);
        
        if (nameWithoutExt.Length > 100)
        {
            nameWithoutExt = nameWithoutExt.Substring(0, 100);
        }
        
        // اضافه کردن timestamp برای یکتایی
        var timestamp = DateTime.UtcNow.Ticks;
        return $"{nameWithoutExt}_{timestamp}{ext}";
    }
}




