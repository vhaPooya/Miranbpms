using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Automation.Infrastructure.Services.Security;

/// <summary>
/// سرویس احراز هویت دو عاملی برای امنیت بالاتر سیستم
/// </summary>
public class TwoFactorAuthenticationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TwoFactorAuthenticationService> _logger;

    public TwoFactorAuthenticationService(IMemoryCache cache, ILogger<TwoFactorAuthenticationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// تولید رمز عبور یکبار مصرف (TOTP)
    /// </summary>
    public string GenerateTotp(string secret, int digits = 6, int timeStep = 30)
    {
        try
        {
            var time = GetCurrentTimeStep(timeStep);
            var key = Base32Decode(secret);
            var hmac = HMACSHA1.HashData(key, BitConverter.GetBytes(time));
            
            // استخراج 4 بایت آخر
            var offset = hmac[hmac.Length - 1] & 0x0F;
            var binary = BitConverter.ToUInt32(hmac, offset) & 0x7FFFFFFF;
            var otp = binary % (uint)Math.Pow(10, digits);
            
            return otp.ToString().PadLeft(digits, '0');
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate TOTP");
            throw new SecurityException("Failed to generate TOTP", ex);
        }
    }

    /// <summary>
    /// تولید رمز عبور یکبار مصرف برای ایمیل/پیامک
    /// </summary>
    public async Task<string> GenerateOneTimePasswordAsync(int userId, string deliveryMethod = "email")
    {
        try
        {
            var otp = GenerateRandomDigits(6);
            var cacheKey = $"2fa_otp_{userId}_{deliveryMethod}";
            
            // ذخیره در کش با انقضای 5 دقیقه
            _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));
            
            _logger.LogInformation($"Generated OTP for user {userId} via {deliveryMethod}");
            return otp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate OTP for user {userId}");
            throw new SecurityException("Failed to generate OTP", ex);
        }
    }

    /// <summary>
    /// تأیید رمز عبور یکبار مصرف
    /// </summary>
    public async Task<bool> VerifyOneTimePasswordAsync(int userId, string otp, string deliveryMethod = "email")
    {
        try
        {
            var cacheKey = $"2fa_otp_{userId}_{deliveryMethod}";
            
            if (_cache.TryGetValue<string>(cacheKey, out var cachedOtp))
            {
                if (cachedOtp == otp)
                {
                    // حذف OTP از کش پس از استفاده
                    _cache.Remove(cacheKey);
                    _logger.LogInformation($"OTP verified successfully for user {userId}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Invalid OTP attempt for user {userId}");
                    return false;
                }
            }
            
            _logger.LogWarning($"OTP not found or expired for user {userId}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to verify OTP for user {userId}");
            return false;
        }
    }

    /// <summary>
    /// تولید کلید مخفی برای TOTP
    /// </summary>
    public string GenerateSecretKey(int length = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var random = new Random();
        var secret = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            secret[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(secret);
    }

    /// <summary>
    /// تولید QR Code برای Google Authenticator
    /// </summary>
    public string GenerateQrCodeUrl(string secret, string issuer, string account)
    {
        var encodedSecret = Uri.EscapeDataString(secret);
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedAccount = Uri.EscapeDataString(account);
        
        return $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={encodedSecret}&issuer={encodedIssuer}";
    }

    /// <summary>
    /// فعال‌سازی 2FA برای کاربر
    /// </summary>
    public async Task<TwoFactorSetupResult> EnableTwoFactorAsync(int userId, TwoFactorMethod method, string secret = null)
    {
        try
        {
            // اگر کلید مخفی داده نشده، یکی تولید کن
            if (string.IsNullOrEmpty(secret))
            {
                secret = GenerateSecretKey();
            }

            var result = new TwoFactorSetupResult
            {
                UserId = userId,
                Method = method,
                SecretKey = secret,
                IsSuccessful = true,
                QrCodeUrl = method == TwoFactorMethod.Authenticator 
                    ? GenerateQrCodeUrl(secret, "EBPMS", $"user{userId}") 
                    : null
            };

            // ذخیره تنظیمات در دیتابیس (این بخش باید در سرویس دیتابیس پیاده شود)
            _logger.LogInformation($"2FA enabled for user {userId} with method {method}");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to enable 2FA for user {userId}");
            return new TwoFactorSetupResult
            {
                UserId = userId,
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// غیرفعال‌سازی 2FA برای کاربر
    /// </summary>
    public async Task<bool> DisableTwoFactorAsync(int userId)
    {
        try
        {
            // حذف تنظیمات از دیتابیس
            _logger.LogInformation($"2FA disabled for user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to disable 2FA for user {userId}");
            return false;
        }
    }

    /// <summary>
    /// ارسال کد 2FA از طریق ایمیل یا پیامک
    /// </summary>
    public async Task<bool> SendTwoFactorCodeAsync(int userId, string deliveryMethod, string code)
    {
        try
        {
            // اینجا باید سرویس ارسال ایمیل یا پیامک فراخوانی شود
            switch (deliveryMethod.ToLower())
            {
                case "email":
                    // ارسال از طریق سرویس ایمیل
                    await SendEmailCodeAsync(userId, code);
                    break;
                case "sms":
                    // ارسال از طریق سرویس پیامک
                    await SendSmsCodeAsync(userId, code);
                    break;
                default:
                    throw new ArgumentException("Invalid delivery method");
            }
            
            _logger.LogInformation($"2FA code sent to user {userId} via {deliveryMethod}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send 2FA code to user {userId}");
            return false;
        }
    }

    /// <summary>
    /// ارسال کد از طریق ایمیل
    /// </summary>
    private async Task SendEmailCodeAsync(int userId, string code)
    {
        // اینجا باید سرویس ایمیل فراخوانی شود
        await Task.CompletedTask;
        // مثلاً: await _emailService.SendAsync(userId, "کد احراز هویت شما", $"کد شما: {code}");
    }

    /// <summary>
    /// ارسال کد از طریق پیامک
    /// </summary>
    private async Task SendSmsCodeAsync(int userId, string code)
    {
        // اینجا باید سرویس پیامک فراخوانی شود
        await Task.CompletedTask;
        // مثلاً: await _smsService.SendAsync(userId, $"کد احراز هویت شما: {code}");
    }

    /// <summary>
    /// دریافت مرحله زمانی فعلی
    /// </summary>
    private ulong GetCurrentTimeStep(int timeStep)
    {
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (ulong)(unixTimestamp / timeStep);
    }

    /// <summary>
    /// رمزگشایی Base32
    /// </summary>
    private byte[] Base32Decode(string input)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        input = input.TrimEnd('=').ToUpper();
        
        var output = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;
        
        foreach (char c in input)
        {
            var value = base32Chars.IndexOf(c);
            if (value == -1)
                throw new ArgumentException("Invalid Base32 character");
                
            buffer = (buffer << 5) | value;
            bitsLeft += 5;
            
            if (bitsLeft >= 8)
            {
                output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }
        
        return output.ToArray();
    }

    /// <summary>
    /// تولید ارقام تصادفی
    /// </summary>
    private string GenerateRandomDigits(int length)
    {
        var random = new Random();
        var sb = new StringBuilder();
        
        for (int i = 0; i < length; i++)
        {
            sb.Append(random.Next(0, 10));
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// بررسی تعداد تلاش‌های ناموفق
    /// </summary>
    public async Task<bool> CheckFailedAttemptsAsync(int userId, int maxAttempts = 3)
    {
        var cacheKey = $"2fa_attempts_{userId}";
        
        if (_cache.TryGetValue<int>(cacheKey, out var attempts))
        {
            return attempts < maxAttempts;
        }
        
        return true;
    }

    /// <summary>
    /// افزایش تعداد تلاش‌های ناموفق
    /// </summary>
    public async Task IncrementFailedAttemptsAsync(int userId)
    {
        var cacheKey = $"2fa_attempts_{userId}";
        var attempts = 1;
        
        if (_cache.TryGetValue<int>(cacheKey, out var currentAttempts))
        {
            attempts = currentAttempts + 1;
        }
        
        // انقضای 15 دقیقه
        _cache.Set(cacheKey, attempts, TimeSpan.FromMinutes(15));
    }

    /// <summary>
    /// ریست تلاش‌های ناموفق
    /// </summary>
    public async Task ResetFailedAttemptsAsync(int userId)
    {
        var cacheKey = $"2fa_attempts_{userId}";
        _cache.Remove(cacheKey);
    }
}

/// <summary>
/// نتیجه تنظیم 2FA
/// </summary>
public class TwoFactorSetupResult
{
    public int UserId { get; set; }
    public TwoFactorMethod Method { get; set; }
    public string SecretKey { get; set; }
    public string QrCodeUrl { get; set; }
    public bool IsSuccessful { get; set; }
    public string ErrorMessage { get; set; }
}

/// <summary>
/// روش‌های احراز هویت دو عاملی
/// </summary>
public enum TwoFactorMethod
{
    Authenticator,
    Email,
    Sms,
    BackupCodes
}

/// <summary>
/// استثناهای امنیتی
/// </summary>
public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
    
    public SecurityException(string message, Exception innerException) 
        : base(message, innerException) { }
}