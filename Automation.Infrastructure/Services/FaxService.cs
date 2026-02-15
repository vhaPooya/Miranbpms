using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت فکس
/// </summary>
public class FaxService : IFaxService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<FaxService> _logger;

    public FaxService(AutomationDbContext context, ILogger<FaxService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FaxSendResult> SendFaxAsync(int settingsId, string toFaxNumber, string filePath, string? fromFaxNumber = null, string? fromName = null, string? toName = null, int? documentId = null, int? senderUserId = null)
    {
        try
        {
            var settings = await _context.Set<FaxServiceSettings>()
                .FirstOrDefaultAsync(s => s.Id == settingsId && s.IsActive && !s.IsDeleted);

            if (settings == null)
            {
                return new FaxSendResult
                {
                    Success = false,
                    ErrorMessage = "تنظیمات فکس یافت نشد"
                };
            }

            if (!File.Exists(filePath))
            {
                return new FaxSendResult
                {
                    Success = false,
                    ErrorMessage = "فایل فکس یافت نشد"
                };
            }

            // ایجاد رکورد در دیتابیس
            var faxMessage = new FaxMessage
            {
                FaxServiceSettingsId = settingsId,
                DocumentId = documentId,
                MessageType = "OUTGOING",
                FromFaxNumber = fromFaxNumber ?? settings.DefaultFaxNumber,
                FromName = fromName,
                ToFaxNumber = toFaxNumber,
                ToName = toName,
                FilePath = filePath,
                Status = "PENDING",
                CreatedByUserId = senderUserId
            };

            _context.Set<FaxMessage>().Add(faxMessage);
            await _context.SaveChangesAsync();

            // ارسال فکس از طریق Gateway
            // TODO: پیاده‌سازی ارسال فکس بر اساس GatewayType
            // این بخش بستگی به Gateway مورد استفاده دارد
            // می‌توان از API Gateway استفاده کرد یا از پروتکل T38/SIP

            // شبیه‌سازی ارسال موفق
            faxMessage.Status = "SENT";
            faxMessage.SentReceivedDateTime = DateTime.UtcNow;
            faxMessage.GatewayMessageId = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Fax sent successfully: {FaxMessageId}", faxMessage.Id);

            return new FaxSendResult
            {
                Success = true,
                FaxMessageId = faxMessage.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending fax");
            return new FaxSendResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<FaxMessageDto>> ReceiveFaxesAsync(int settingsId, int maxCount = 50)
    {
        // TODO: پیاده‌سازی دریافت فکس از Gateway
        _logger.LogWarning("ReceiveFaxesAsync not implemented yet");
        return new List<FaxMessageDto>();
    }

    public async Task<List<FaxMessageDto>> GetFaxMessagesAsync(int? settingsId = null, string? messageType = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Set<FaxMessage>()
                .Where(f => !f.IsDeleted);

            if (settingsId.HasValue)
            {
                query = query.Where(f => f.FaxServiceSettingsId == settingsId.Value);
            }

            if (!string.IsNullOrEmpty(messageType))
            {
                query = query.Where(f => f.MessageType == messageType);
            }

            var messages = await query
                .OrderByDescending(f => f.SentReceivedDateTime ?? f.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FaxMessageDto
                {
                    Id = f.Id,
                    DocumentId = f.DocumentId,
                    MessageType = f.MessageType,
                    FromFaxNumber = f.FromFaxNumber,
                    FromName = f.FromName,
                    ToFaxNumber = f.ToFaxNumber,
                    ToName = f.ToName,
                    FilePath = f.FilePath,
                    PageCount = f.PageCount,
                    Status = f.Status,
                    SentReceivedDateTime = f.SentReceivedDateTime,
                    CreatedDateTime = f.CreationDate
                })
                .ToListAsync();

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fax messages");
            return new List<FaxMessageDto>();
        }
    }

    public async Task<FaxMessageDto?> GetFaxMessageAsync(int faxMessageId)
    {
        try
        {
            var message = await _context.Set<FaxMessage>()
                .Where(f => f.Id == faxMessageId && !f.IsDeleted)
                .Select(f => new FaxMessageDto
                {
                    Id = f.Id,
                    DocumentId = f.DocumentId,
                    MessageType = f.MessageType,
                    FromFaxNumber = f.FromFaxNumber,
                    FromName = f.FromName,
                    ToFaxNumber = f.ToFaxNumber,
                    ToName = f.ToName,
                    FilePath = f.FilePath,
                    PageCount = f.PageCount,
                    Status = f.Status,
                    SentReceivedDateTime = f.SentReceivedDateTime,
                    CreatedDateTime = f.CreationDate
                })
                .FirstOrDefaultAsync();

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fax message {FaxMessageId}", faxMessageId);
            return null;
        }
    }
}




