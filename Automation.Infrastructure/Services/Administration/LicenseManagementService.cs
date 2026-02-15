using System.Security.Cryptography;
using System.Text;

namespace Automation.Infrastructure.Services.Administration;

/// <summary>
/// سرویس مدیریت لایسنس برای کنترل استفاده از نرم‌افزار
/// </summary>
public class LicenseManagementService
{
    private readonly ILogger<LicenseManagementService> _logger;
    private readonly string _licensePublicKey;

    public LicenseManagementService(ILogger<LicenseManagementService> logger, string licensePublicKey)
    {
        _logger = logger;
        _licensePublicKey = licensePublicKey;
    }

    /// <summary>
    /// اعتبارسنجی لایسنس
    /// </summary>
    public async Task<LicenseValidationResult> ValidateLicenseAsync(LicenseInfo license)
    {
        try
        {
            var validationResult = new LicenseValidationResult
            {
                IsValid = false,
                ValidationDate = DateTime.UtcNow
            };

            // بررسی امضای دیجیتال
            if (!VerifyDigitalSignature(license, _licensePublicKey))
            {
                validationResult.ErrorMessage = "Invalid license signature";
                return validationResult;
            }

            // بررسی تاریخ انقضا
            if (license.ExpirationDate < DateTime.UtcNow)
            {
                validationResult.ErrorMessage = "License has expired";
                validationResult.IsExpired = true;
                return validationResult;
            }

            // بررسی تعداد کاربران
            if (license.UserLimit > 0 && license.ActivatedUsers > license.UserLimit)
            {
                validationResult.ErrorMessage = "User limit exceeded";
                return validationResult;
            }

            // بررسی ویژگی‌های فعال
            if (!ValidateFeatures(license))
            {
                validationResult.ErrorMessage = "Feature validation failed";
                return validationResult;
            }

            validationResult.IsValid = true;
            validationResult.ValidUntil = license.ExpirationDate;
            validationResult.RemainingDays = (license.ExpirationDate - DateTime.UtcNow).Days;

            _logger.LogInformation($"License validated successfully for customer: {license.CustomerName}");
            return validationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate license");
            return new LicenseValidationResult
            {
                IsValid = false,
                ErrorMessage = "License validation error",
                ValidationDate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// تولید لایسنس جدید
    /// </summary>
    public LicenseInfo GenerateLicense(LicenseRequest request, string privateKey)
    {
        var license = new LicenseInfo
        {
            LicenseId = Guid.NewGuid().ToString("N"),
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerOrganization = request.CustomerOrganization,
            LicenseType = request.LicenseType,
            UserLimit = request.UserLimit,
            ExpirationDate = request.ExpirationDate,
            Features = request.Features,
            GeneratedDate = DateTime.UtcNow,
            IssuedBy = "EBPMS Licensing System"
        };

        // امضای دیجیتال
        license.Signature = GenerateDigitalSignature(license, privateKey);
        
        return license;
    }

    /// <summary>
    /// امضای دیجیتال لایسنس
    /// </summary>
    private string GenerateDigitalSignature(LicenseInfo license, string privateKey)
    {
        try
        {
            // سریالایز کردن داده‌های لایسنس
            var licenseData = $"{license.LicenseId}|{license.CustomerName}|{license.CustomerEmail}|" +
                             $"{license.CustomerOrganization}|{license.LicenseType}|{license.UserLimit}|" +
                             $"{license.ExpirationDate:yyyy-MM-dd}|{string.Join(",", license.Features)}";

            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKey);
            
            var dataBytes = Encoding.UTF8.GetBytes(licenseData);
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            return Convert.ToBase64String(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate digital signature");
            throw;
        }
    }

    /// <summary>
    /// تأیید امضای دیجیتال لایسنس
    /// </summary>
    private bool VerifyDigitalSignature(LicenseInfo license, string publicKey)
    {
        try
        {
            // سریالایز کردن داده‌های لایسنس
            var licenseData = $"{license.LicenseId}|{license.CustomerName}|{license.CustomerEmail}|" +
                             $"{license.CustomerOrganization}|{license.LicenseType}|{license.UserLimit}|" +
                             $"{license.ExpirationDate:yyyy-MM-dd}|{string.Join(",", license.Features)}";

            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKey);
            
            var dataBytes = Encoding.UTF8.GetBytes(licenseData);
            var signatureBytes = Convert.FromBase64String(license.Signature);
            
            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify digital signature");
            return false;
        }
    }

    /// <summary>
    /// اعتبارسنجی ویژگی‌ها
    /// </summary>
    private bool ValidateFeatures(LicenseInfo license)
    {
        // بررسی ویژگی‌های ضروری
        var requiredFeatures = new[] { "core", "ui", "workflow", "documents" };
        
        foreach (var required in requiredFeatures)
        {
            if (!license.Features.Contains(required))
            {
                _logger.LogWarning($"Required feature missing: {required}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// فعال‌سازی لایسنس برای کاربر
    /// </summary>
    public async Task<bool> ActivateLicenseForUserAsync(string licenseId, int userId)
    {
        try
        {
            // در اینجا باید لایسنس را در دیتابیس به‌روزرسانی کنیم
            _logger.LogInformation($"License activated for user {userId} with license {licenseId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to activate license for user {userId}");
            return false;
        }
    }

    /// <summary>
    /// دریافت اطلاعات لایسنس
    /// </summary>
    public async Task<LicenseInfo> GetLicenseInfoAsync(string licenseId)
    {
        try
        {
            // در اینجا باید لایسنس را از دیتابیس دریافت کنیم
            // برای سادگی یک نمونه برمی‌گردانیم
            return new LicenseInfo
            {
                LicenseId = licenseId,
                CustomerName = "Sample Customer",
                CustomerEmail = "customer@example.com",
                CustomerOrganization = "Sample Organization",
                LicenseType = LicenseType.Enterprise,
                UserLimit = 100,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                Features = new List<string> { "core", "ui", "workflow", "documents", "reports", "integration" },
                GeneratedDate = DateTime.UtcNow.AddMonths(-1),
                IssuedBy = "EBPMS Licensing System",
                ActivatedUsers = 15
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get license info for {licenseId}");
            return null;
        }
    }

    /// <summary>
    /// دریافت آمار استفاده از لایسنس
    /// </summary>
    public async Task<LicenseUsageReport> GetLicenseUsageReportAsync(string licenseId)
    {
        try
        {
            // در اینجا باید آمار استفاده را از دیتابیس دریافت کنیم
            return new LicenseUsageReport
            {
                LicenseId = licenseId,
                ReportDate = DateTime.UtcNow,
                ActiveUsers = 15,
                TotalUsers = 100,
                FeatureUsage = new Dictionary<string, int>
                {
                    { "core", 15 },
                    { "workflow", 12 },
                    { "documents", 14 },
                    { "reports", 8 },
                    { "integration", 5 }
                },
                DailyUsage = new List<DailyUsageStat>
                {
                    new DailyUsageStat { Date = DateTime.UtcNow.AddDays(-6), ActiveUsers = 12 },
                    new DailyUsageStat { Date = DateTime.UtcNow.AddDays(-5), ActiveUsers = 13 },
                    new DailyUsageStat { Date = DateTime.UtcNow.AddDays(-4), ActiveUsers = 14 },
                    new DailyUsageStat { Date = DateTime.UtcNow.AddDays(-3), ActiveUsers = 15 },
                    new DailyUsageStat { Date = DateTime.UtcNow.AddDays(-2), ActiveUsers = 14 },
                    new DailyUsageStat { Date = DateTime.UtcNow.AddDays(-1), ActiveUsers = 15 },
                    new DailyUsageStat { Date = DateTime.UtcNow, ActiveUsers = 15 }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get license usage report for {licenseId}");
            return null;
        }
    }

    /// <summary>
    /// تمدید لایسنس
    /// </summary>
    public async Task<LicenseRenewalResult> RenewLicenseAsync(string licenseId, int months, string privateKey)
    {
        try
        {
            var license = await GetLicenseInfoAsync(licenseId);
            if (license == null)
            {
                return new LicenseRenewalResult
                {
                    Success = false,
                    ErrorMessage = "License not found"
                };
            }

            // تمدید تاریخ انقضا
            license.ExpirationDate = license.ExpirationDate.AddMonths(months);
            
            // امضای مجدد
            license.Signature = GenerateDigitalSignature(license, privateKey);

            // ذخیره لایسنس تمدید شده
            // در اینجا باید در دیتابیس ذخیره شود

            _logger.LogInformation($"License {licenseId} renewed for {months} months");

            return new LicenseRenewalResult
            {
                Success = true,
                NewExpirationDate = license.ExpirationDate,
                NewLicense = license
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to renew license {licenseId}");
            return new LicenseRenewalResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// غیرفعال‌سازی لایسنس
    /// </summary>
    public async Task<bool> DeactivateLicenseAsync(string licenseId)
    {
        try
        {
            // در اینجا باید لایسنس را در دیتابیس غیرفعال کنیم
            _logger.LogInformation($"License {licenseId} deactivated");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to deactivate license {licenseId}");
            return false;
        }
    }

    /// <summary>
    /// بررسی نیاز به تمدید
    /// </summary>
    public async Task<bool> CheckRenewalNeededAsync(string licenseId)
    {
        try
        {
            var license = await GetLicenseInfoAsync(licenseId);
            if (license == null) return false;

            // اگر کمتر از 30 روز تا انقضا باقی مانده باشد
            return (license.ExpirationDate - DateTime.UtcNow).Days <= 30;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check renewal for license {licenseId}");
            return false;
        }
    }
}

/// <summary>
/// اطلاعات لایسنس
/// </summary>
public class LicenseInfo
{
    public string LicenseId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerOrganization { get; set; }
    public LicenseType LicenseType { get; set; }
    public int UserLimit { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<string> Features { get; set; } = new List<string>();
    public DateTime GeneratedDate { get; set; }
    public string IssuedBy { get; set; }
    public string Signature { get; set; }
    public int ActivatedUsers { get; set; }
}

/// <summary>
/// انواع لایسنس
/// </summary>
public enum LicenseType
{
    Trial,
    Standard,
    Professional,
    Enterprise,
    Unlimited
}

/// <summary>
/// نتیجه اعتبارسنجی لایسنس
/// </summary>
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime ValidationDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int? RemainingDays { get; set; }
}

/// <summary>
/// درخواست لایسنس
/// </summary>
public class LicenseRequest
{
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerOrganization { get; set; }
    public LicenseType LicenseType { get; set; }
    public int UserLimit { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<string> Features { get; set; } = new List<string>();
}

/// <summary>
/// گزارش استفاده از لایسنس
/// </summary>
public class LicenseUsageReport
{
    public string LicenseId { get; set; }
    public DateTime ReportDate { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new Dictionary<string, int>();
    public List<DailyUsageStat> DailyUsage { get; set; } = new List<DailyUsageStat>();
}

/// <summary>
/// آمار استفاده روزانه
/// </summary>
public class DailyUsageStat
{
    public DateTime Date { get; set; }
    public int ActiveUsers { get; set; }
}

/// <summary>
/// نتیجه تمدید لایسنس
/// </summary>
public class LicenseRenewalResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime? NewExpirationDate { get; set; }
    public LicenseInfo NewLicense { get; set; }
}