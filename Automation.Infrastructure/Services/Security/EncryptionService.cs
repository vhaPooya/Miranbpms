using System.Security.Cryptography;
using System.Text;

namespace Automation.Infrastructure.Services.Security;

/// <summary>
/// سرویس رمزگذاری پیشرفته برای امنیت اطلاعات
/// </summary>
public class EncryptionService
{
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(ILogger<EncryptionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// رمزگذاری داده با AES
    /// </summary>
    public EncryptedData EncryptAes(string plainText, string key, string iv = null)
    {
        try
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // اگر IV داده نشده، یکی تولید کن
            if (string.IsNullOrEmpty(iv))
            {
                aes.GenerateIV();
            }
            else
            {
                aes.IV = Convert.FromBase64String(iv);
            }

            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var encrypted = ms.ToArray();
            
            return new EncryptedData
            {
                EncryptedContent = Convert.ToBase64String(encrypted),
                Iv = Convert.ToBase64String(aes.IV),
                Algorithm = "AES-256-CBC"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data with AES");
            throw new SecurityException("Failed to encrypt data", ex);
        }
    }

    /// <summary>
    /// رمزگشایی داده با AES
    /// </summary>
    public string DecryptAes(EncryptedData encryptedData, string key)
    {
        try
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            aes.IV = Convert.FromBase64String(encryptedData.Iv);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encryptedData.EncryptedContent));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data with AES");
            throw new SecurityException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// امضای دیجیتال با RSA
    /// </summary>
    public DigitalSignature SignData(string data, string privateKeyXml)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml);
            
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            return new DigitalSignature
            {
                Signature = Convert.ToBase64String(signature),
                Algorithm = "RSA-SHA256",
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign data with RSA");
            throw new SecurityException("Failed to sign data", ex);
        }
    }

    /// <summary>
    /// تأیید امضای دیجیتال
    /// </summary>
    public bool VerifySignature(string data, DigitalSignature signature, string publicKeyXml)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKeyXml);
            
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature.Signature);
            
            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify digital signature");
            return false;
        }
    }

    /// <summary>
    /// تولید کلید RSA
    /// </summary>
    public RsaKeyPair GenerateRsaKeyPair(int keySize = 2048)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.KeySize = keySize;
            
            return new RsaKeyPair
            {
                PublicKey = rsa.ToXmlString(false),
                PrivateKey = rsa.ToXmlString(true),
                KeySize = keySize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate RSA key pair");
            throw new SecurityException("Failed to generate RSA key pair", ex);
        }
    }

    /// <summary>
    /// هش کردن داده با SHA-256
    /// </summary>
    public string HashSha256(string input)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash data with SHA-256");
            throw new SecurityException("Failed to hash data", ex);
        }
    }

    /// <summary>
    /// هش کردن داده باSalt
    /// </summary>
    public SaltedHash HashWithSalt(string input, string salt = null)
    {
        try
        {
            // اگر Salt داده نشده، یکی تولید کن
            if (string.IsNullOrEmpty(salt))
            {
                salt = GenerateSalt(32);
            }

            var saltedInput = input + salt;
            var hash = HashSha256(saltedInput);
            
            return new SaltedHash
            {
                Hash = hash,
                Salt = salt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash data with salt");
            throw new SecurityException("Failed to hash data with salt", ex);
        }
    }

    /// <summary>
    /// تولید Salt تصادفی
    /// </summary>
    public string GenerateSalt(int length)
    {
        var random = new Random();
        var salt = new byte[length];
        random.NextBytes(salt);
        return Convert.ToBase64String(salt);
    }

    /// <summary>
    /// رمزنگاری داده با کلید عمومی
    /// </summary>
    public EncryptedData EncryptWithPublicKey(string plainText, string publicKeyXml)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(publicKeyXml);
            
            var dataBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
            
            return new EncryptedData
            {
                EncryptedContent = Convert.ToBase64String(encryptedBytes),
                Algorithm = "RSA-OAEP"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt with public key");
            throw new SecurityException("Failed to encrypt with public key", ex);
        }
    }

    /// <summary>
    /// رمزگشایی داده با کلید خصوصی
    /// </summary>
    public string DecryptWithPrivateKey(EncryptedData encryptedData, string privateKeyXml)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(privateKeyXml);
            
            var encryptedBytes = Convert.FromBase64String(encryptedData.EncryptedContent);
            var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt with private key");
            throw new SecurityException("Failed to decrypt with private key", ex);
        }
    }

    /// <summary>
    /// تولید توکن امن
    /// </summary>
    public SecureToken GenerateSecureToken(string userId, TimeSpan expiry = default)
    {
        try
        {
            if (expiry == default)
            {
                expiry = TimeSpan.FromHours(8);
            }

            var tokenId = Guid.NewGuid().ToString("N");
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.Add(expiry);
            
            // اطلاعات توکن
            var tokenData = $"{userId}|{tokenId}|{issuedAt.Ticks}|{expiresAt.Ticks}";
            
            // امضای توکن
            var signature = HashSha256(tokenData);
            
            return new SecureToken
            {
                TokenId = tokenId,
                UserId = userId,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt,
                Signature = signature,
                Data = tokenData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate secure token");
            throw new SecurityException("Failed to generate secure token", ex);
        }
    }

    /// <summary>
    /// اعتبارسنجی توکن امن
    /// </summary>
    public bool ValidateSecureToken(SecureToken token)
    {
        try
        {
            // بررسی انقضا
            if (token.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning($"Token expired: {token.TokenId}");
                return false;
            }

            // تأیید امضا
            var expectedSignature = HashSha256(token.Data);
            if (token.Signature != expectedSignature)
            {
                _logger.LogWarning($"Invalid token signature: {token.TokenId}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to validate token: {token.TokenId}");
            return false;
        }
    }

    /// <summary>
    /// رمزنگاری فایل
    /// </summary>
    public async Task<EncryptedFile> EncryptFileAsync(string filePath, string key)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var extension = fileInfo.Extension;
            var encryptedFilePath = Path.Combine(fileInfo.DirectoryName, $"{fileName}_encrypted{extension}");

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            aes.GenerateIV();

            using var inputFile = File.OpenRead(filePath);
            using var outputFile = File.Create(encryptedFilePath);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(outputFile, encryptor, CryptoStreamMode.Write);

            await inputFile.CopyToAsync(cryptoStream);

            return new EncryptedFile
            {
                OriginalFileName = fileInfo.Name,
                EncryptedFilePath = encryptedFilePath,
                Iv = Convert.ToBase64String(aes.IV),
                FileSize = fileInfo.Length,
                EncryptionDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to encrypt file: {filePath}");
            throw new SecurityException("Failed to encrypt file", ex);
        }
    }

    /// <summary>
    /// رمزگشایی فایل
    /// </summary>
    public async Task<string> DecryptFileAsync(EncryptedFile encryptedFile, string key, string outputPath = null)
    {
        try
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(encryptedFile.OriginalFileName);
                var extension = Path.GetExtension(encryptedFile.OriginalFileName);
                outputPath = Path.Combine(Path.GetDirectoryName(encryptedFile.EncryptedFilePath), $"{fileName}_decrypted{extension}");
            }

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            aes.IV = Convert.FromBase64String(encryptedFile.Iv);

            using var inputFile = File.OpenRead(encryptedFile.EncryptedFilePath);
            using var outputFile = File.Create(outputPath);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(inputFile, decryptor, CryptoStreamMode.Read);

            await cryptoStream.CopyToAsync(outputFile);

            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to decrypt file: {encryptedFile.EncryptedFilePath}");
            throw new SecurityException("Failed to decrypt file", ex);
        }
    }
}

/// <summary>
/// داده رمزگذاری شده
/// </summary>
public class EncryptedData
{
    public string EncryptedContent { get; set; }
    public string Iv { get; set; }
    public string Algorithm { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// امضای دیجیتال
/// </summary>
public class DigitalSignature
{
    public string Signature { get; set; }
    public string Algorithm { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// جفت کلید RSA
/// </summary>
public class RsaKeyPair
{
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public int KeySize { get; set; }
}

/// <summary>
/// هش با Salt
/// </summary>
public class SaltedHash
{
    public string Hash { get; set; }
    public string Salt { get; set; }
}

/// <summary>
/// توکن امن
/// </summary>
public class SecureToken
{
    public string TokenId { get; set; }
    public string UserId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Signature { get; set; }
    public string Data { get; set; }
}

/// <summary>
/// فایل رمزگذاری شده
/// </summary>
public class EncryptedFile
{
    public string OriginalFileName { get; set; }
    public string EncryptedFilePath { get; set; }
    public string Iv { get; set; }
    public long FileSize { get; set; }
    public DateTime EncryptionDate { get; set; }
}