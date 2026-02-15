using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت ایمیل
/// </summary>
public class EmailService : IEmailService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(AutomationDbContext context, ILogger<EmailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendEmailAsync(int settingsId, string toEmail, string subject, string body, bool isHtml = false, string? fromEmail = null, string? fromName = null, string? cc = null, string? bcc = null, List<string>? attachments = null, int? documentId = null, int? senderUserId = null)
    {
        try
        {
            var settings = await _context.Set<EmailServiceSettings>()
                .FirstOrDefaultAsync(s => s.Id == settingsId && s.IsActive && !s.IsDeleted);

            if (settings == null)
            {
                return new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = "تنظیمات ایمیل یافت نشد"
                };
            }

            // ایجاد رکورد در دیتابیس
            var emailMessage = new EmailMessage
            {
                EmailServiceSettingsId = settingsId,
                DocumentId = documentId,
                MessageType = "OUTGOING",
                FromEmail = fromEmail ?? settings.FromEmail,
                FromName = fromName ?? settings.FromName,
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
                IsHtml = isHtml,
                Cc = cc,
                Bcc = bcc,
                Status = "PENDING",
                CreatedByUserId = senderUserId
            };

            _context.Set<EmailMessage>().Add(emailMessage);
            await _context.SaveChangesAsync();

            // ارسال ایمیل
            using (var client = new SmtpClient(settings.SmtpServer, settings.SmtpPort))
            {
                client.EnableSsl = settings.UseSsl;
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(settings.FromEmail, settings.FromName);
                    message.To.Add(toEmail);
                    
                    if (!string.IsNullOrEmpty(cc))
                    {
                        message.CC.Add(cc);
                    }
                    
                    if (!string.IsNullOrEmpty(bcc))
                    {
                        message.Bcc.Add(bcc);
                    }
                    
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;

                    // اضافه کردن ضمائم
                    if (attachments != null)
                    {
                        foreach (var attachment in attachments)
                        {
                            if (File.Exists(attachment))
                            {
                                message.Attachments.Add(new Attachment(attachment));
                            }
                        }
                    }

                    await client.SendMailAsync(message);
                }
            }

            // به‌روزرسانی وضعیت
            emailMessage.Status = "SENT";
            emailMessage.SentReceivedDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email sent successfully: {EmailMessageId}", emailMessage.Id);

            return new EmailSendResult
            {
                Success = true,
                EmailMessageId = emailMessage.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<EmailMessageDto>> ReceiveEmailsAsync(int settingsId, int maxCount = 50)
    {
        // TODO: پیاده‌سازی دریافت ایمیل از IMAP/POP3
        _logger.LogWarning("ReceiveEmailsAsync not implemented yet");
        return new List<EmailMessageDto>();
    }

    public async Task<List<EmailMessageDto>> GetEmailMessagesAsync(int? settingsId = null, string? messageType = null, string? status = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Set<EmailMessage>()
                .Where(e => !e.IsDeleted);

            if (settingsId.HasValue)
            {
                query = query.Where(e => e.EmailServiceSettingsId == settingsId.Value);
            }

            if (!string.IsNullOrEmpty(messageType))
            {
                query = query.Where(e => e.MessageType == messageType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(e => e.Status == status);
            }

            var messages = await query
                .OrderByDescending(e => e.SentReceivedDateTime ?? e.CreationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmailMessageDto
                {
                    Id = e.Id,
                    DocumentId = e.DocumentId,
                    MessageType = e.MessageType,
                    FromEmail = e.FromEmail,
                    FromName = e.FromName,
                    ToEmail = e.ToEmail,
                    ToName = e.ToName,
                    Subject = e.Subject,
                    Body = e.Body,
                    Status = e.Status,
                    SentReceivedDateTime = e.SentReceivedDateTime,
                    CreatedDateTime = e.CreationDate
                })
                .ToListAsync();

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email messages");
            return new List<EmailMessageDto>();
        }
    }

    public async Task<EmailMessageDto?> GetEmailMessageAsync(int emailMessageId)
    {
        try
        {
            var message = await _context.Set<EmailMessage>()
                .Where(e => e.Id == emailMessageId && !e.IsDeleted)
                .Select(e => new EmailMessageDto
                {
                    Id = e.Id,
                    DocumentId = e.DocumentId,
                    MessageType = e.MessageType,
                    FromEmail = e.FromEmail,
                    FromName = e.FromName,
                    ToEmail = e.ToEmail,
                    ToName = e.ToName,
                    Subject = e.Subject,
                    Body = e.Body,
                    Status = e.Status,
                    SentReceivedDateTime = e.SentReceivedDateTime,
                    CreatedDateTime = e.CreationDate
                })
                .FirstOrDefaultAsync();

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email message {EmailMessageId}", emailMessageId);
            return null;
        }
    }
}




