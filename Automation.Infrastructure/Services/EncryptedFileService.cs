using System.Security.Cryptography;

namespace Automation.Infrastructure.Services;

/// <summary>
/// Interface for encrypted file storage service
/// </summary>
public interface IEncryptedFileService
{
    /// <summary>
    /// Save a file with AES-256 encryption
    /// </summary>
    /// <param name="fileStream">Input file stream</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="formId">Form ID for folder organization</param>
    /// <returns>Encrypted file path</returns>
    Task<string> SaveEncryptedAsync(Stream fileStream, string fileName, int formId);

    /// <summary>
    /// Get decrypted file stream
    /// </summary>
    /// <param name="encryptedPath">Path to encrypted file</param>
    /// <returns>Decrypted file stream</returns>
    Task<Stream> GetDecryptedAsync(string encryptedPath);

    /// <summary>
    /// Delete encrypted file
    /// </summary>
    /// <param name="encryptedPath">Path to encrypted file</param>
    Task DeleteAsync(string encryptedPath);

    /// <summary>
    /// Get the base URL for serving files
    /// </summary>
    string GetFileUrl(string encryptedPath);
}

/// <summary>
/// Implementation of encrypted file storage using AES-256
/// </summary>
public class EncryptedFileService : IEncryptedFileService
{
    private readonly string _basePath;
    private readonly byte[] _encryptionKey;
    private readonly byte[] _iv;
    private const string EncryptedExtension = ".enc";

    public EncryptedFileService(string basePath, string encryptionKey)
    {
        _basePath = basePath;
        
        // Ensure base path exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }

        // Derive a 256-bit key from the provided key using SHA256
        using var sha256 = SHA256.Create();
        _encryptionKey = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(encryptionKey));
        
        // Use first 16 bytes of key hash as IV (for simplicity, in production use random IV stored with file)
        _iv = _encryptionKey.Take(16).ToArray();
    }

    public async Task<string> SaveEncryptedAsync(Stream fileStream, string fileName, int formId)
    {
        // Create form-specific folder
        var formFolder = Path.Combine(_basePath, formId.ToString());
        if (!Directory.Exists(formFolder))
        {
            Directory.CreateDirectory(formFolder);
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}{EncryptedExtension}";
        var filePath = Path.Combine(formFolder, uniqueFileName);

        // Encrypt and save
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var fileStreamOut = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var encryptor = aes.CreateEncryptor();
        using var cryptoStream = new CryptoStream(fileStreamOut, encryptor, CryptoStreamMode.Write);
        
        await fileStream.CopyToAsync(cryptoStream);
        await cryptoStream.FlushFinalBlockAsync();

        // Return relative path for storage in database
        return Path.Combine(formId.ToString(), uniqueFileName);
    }

    public async Task<Stream> GetDecryptedAsync(string encryptedPath)
    {
        var fullPath = Path.Combine(_basePath, encryptedPath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Encrypted file not found: {encryptedPath}");
        }

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptedBytes = await File.ReadAllBytesAsync(fullPath);
        
        using var decryptor = aes.CreateDecryptor();
        using var memoryStreamIn = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStreamIn, decryptor, CryptoStreamMode.Read);
        
        var decryptedStream = new MemoryStream();
        await cryptoStream.CopyToAsync(decryptedStream);
        decryptedStream.Position = 0;
        
        return decryptedStream;
    }

    public Task DeleteAsync(string encryptedPath)
    {
        var fullPath = Path.Combine(_basePath, encryptedPath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GetFileUrl(string encryptedPath)
    {
        // This would typically be a controller route that serves the decrypted file
        return $"/api/files/{encryptedPath.Replace("\\", "/")}";
    }
}

/// <summary>
/// Configuration options for encrypted file service
/// </summary>
public class EncryptedFileServiceOptions
{
    /// <summary>
    /// Base path for storing encrypted files
    /// </summary>
    public string BasePath { get; set; } = "EncryptedFiles";

    /// <summary>
    /// Encryption key (should be stored securely, e.g., in Azure Key Vault or appsettings secrets)
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum file size in bytes (default 50MB)
    /// </summary>
    public long MaxFileSize { get; set; } = 52428800;

    /// <summary>
    /// Allowed file extensions
    /// </summary>
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
}



