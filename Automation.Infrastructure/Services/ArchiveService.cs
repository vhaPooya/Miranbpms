using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت دبیرخانه
/// </summary>
public class ArchiveService : IArchiveService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(AutomationDbContext context, ILogger<ArchiveService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RegisterIncomingResult> RegisterIncomingDocumentAsync(RegisterIncomingDto dto)
    {
        try
        {
            var document = new Document
            {
                FormId = dto.FormId,
                Subject = dto.Subject,
                Content = dto.Content,
                DocumentType = "INCOMING",
                DocumentNumber = dto.IncomingNumber ?? await GetNextIncomingNumberAsync(DateTime.Now.Year, dto.OrganizationId, dto.DepartmentId, dto.SecretariatId, dto.DocumentTypeId),
                Status = "REGISTERED",
                CreatedByUserId = dto.RegisteredByUserId,
                CreatedDateTime = DateTime.UtcNow,
                IsDeleted = false,
                IsSigned = false
            };

            _context.Documents.Add(document);

            // Create IncomingDocument record
            var incomingDoc = new IncomingDocument
            {
                DocumentId = document.Id,
                SenderName = dto.SenderName,
                SenderNumber = dto.SenderNumber,
                SenderDate = dto.SenderDate,
                SenderEmail = dto.SenderEmail,
                SenderAddress = dto.SenderAddress,
                SenderMobile = dto.SenderMobile,
                ReceiptMethod = dto.ReceiptMethod,
                ScannedFilePath = dto.ScannedFilePath,
                IncomingDate = dto.IncomingDate ?? DateTime.UtcNow,
                IncomingNumber = document.DocumentNumber,
                DocumentDate = dto.DocumentDate,
                RegisteredByUserId = dto.RegisteredByUserId,
                RegisteredDateTime = DateTime.UtcNow,
                DocumentTypeId = dto.DocumentTypeId,
                ClassificationId = dto.ClassificationId,
                ReceiptMethodId = dto.ReceiptMethodId,
                UrgencyId = dto.UrgencyId,
                SecretariatId = dto.SecretariatId
            };

            _context.IncomingDocuments.Add(incomingDoc);

            // Add attachments
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                foreach (var attachment in dto.Attachments)
                {
                    var docAttachment = new DocumentAttachment
                    {
                        DocumentId = document.Id,
                        FileName = attachment.FileName,
                        FilePath = attachment.FilePath,
                        FileSize = attachment.FileSize,
                        MimeType = attachment.MimeType,
                        Description = attachment.Description,
                        AttachmentType = "ATTACHMENT",
                        UploadedDate = DateTime.UtcNow
                    };
                    _context.DocumentAttachments.Add(docAttachment);
                }
            }

            await _context.SaveChangesAsync();

            return new RegisterIncomingResult
            {
                Success = true,
                DocumentId = document.Id,
                IncomingNumber = document.DocumentNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering incoming document");
            return new RegisterIncomingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RegisterOutgoingResult> RegisterOutgoingDocumentAsync(RegisterOutgoingDto dto)
    {
        try
        {
            var document = new Document
            {
                FormId = dto.FormId,
                Subject = dto.Subject,
                Content = dto.Content,
                DocumentType = "OUTGOING",
                DocumentNumber = dto.OutgoingNumber ?? await GetNextOutgoingNumberAsync(DateTime.Now.Year, dto.OrganizationId, dto.DepartmentId, dto.SecretariatId, dto.DocumentTypeId),
                Status = "REGISTERED",
                CreatedByUserId = dto.RegisteredByUserId,
                CreatedDateTime = DateTime.UtcNow,
                IsDeleted = false,
                IsSigned = false
            };

            _context.Documents.Add(document);

            // Create OutgoingDocument record
            var outgoingDoc = new OutgoingDocument
            {
                DocumentId = document.Id,
                RecipientName = dto.RecipientName,
                RecipientAddress = dto.RecipientAddress,
                DeliveryMethod = dto.DeliveryMethod,
                OutgoingDate = dto.OutgoingDate ?? DateTime.UtcNow,
                OutgoingNumber = document.DocumentNumber,
                DocumentDate = dto.DocumentDate,
                RegisteredByUserId = dto.RegisteredByUserId,
                RegisteredDateTime = DateTime.UtcNow,
                DocumentTypeId = dto.DocumentTypeId,
                ClassificationId = dto.ClassificationId,
                UrgencyId = dto.UrgencyId,
                SecretariatId = dto.SecretariatId
            };

            _context.OutgoingDocuments.Add(outgoingDoc);

            // Add attachments
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                foreach (var attachment in dto.Attachments)
                {
                    var docAttachment = new DocumentAttachment
                    {
                        DocumentId = document.Id,
                        FileName = attachment.FileName,
                        FilePath = attachment.FilePath,
                        FileSize = attachment.FileSize,
                        MimeType = attachment.MimeType,
                        Description = attachment.Description,
                        AttachmentType = "ATTACHMENT",
                        UploadedDate = DateTime.UtcNow
                    };
                    _context.DocumentAttachments.Add(docAttachment);
                }
            }

            await _context.SaveChangesAsync();

            return new RegisterOutgoingResult
            {
                Success = true,
                DocumentId = document.Id,
                OutgoingNumber = document.DocumentNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering outgoing document");
            return new RegisterOutgoingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string> GetNextIncomingNumberAsync(int year, int? organizationId = null, int? departmentId = null, int? secretariatId = null, int? documentTypeId = null)
    {
        var lastNumber = await _context.IncomingDocuments
            .Where(d => d.IncomingDate.Year == year)
            .OrderByDescending(d => d.IncomingNumber)
            .Select(d => d.IncomingNumber)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(lastNumber))
        {
            return $"{year}/1";
        }

        var parts = lastNumber.Split('/');
        if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out var lastNum))
        {
            return $"{year}/{lastNum + 1}";
        }

        return $"{year}/1";
    }

    public async Task<string> GetNextOutgoingNumberAsync(int year, int? organizationId = null, int? departmentId = null, int? secretariatId = null, int? documentTypeId = null)
    {
        var lastNumber = await _context.OutgoingDocuments
            .Where(d => d.OutgoingDate.Year == year)
            .OrderByDescending(d => d.OutgoingNumber)
            .Select(d => d.OutgoingNumber)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(lastNumber))
        {
            return $"{year}/1";
        }

        var parts = lastNumber.Split('/');
        if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1], out var lastNum))
        {
            return $"{year}/{lastNum + 1}";
        }

        return $"{year}/1";
    }

    public async Task<bool> ArchiveDocumentAsync(int documentId, int userId, string? archiveLocation = null)
    {
        try
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                return false;

            document.IsArchived = true;
            document.ArchivedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<ArchiveListDto> GetArchivedDocumentsAsync(int page = 1, int pageSize = 10, string? search = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Documents
            .Where(d => !d.IsDeleted && d.IsArchived == true);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Subject.Contains(search) || d.DocumentNumber.Contains(search));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(d => d.CreatedDateTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(d => d.CreatedDateTime <= toDate.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.ArchivedDateTime ?? d.CreatedDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new ArchiveItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                CreatedDateTime = d.CreatedDateTime,
                ArchivedDateTime = d.ArchivedDateTime ?? d.CreatedDateTime,
                ArchiveLocation = null // ArchiveLocation field doesn't exist in Document entity
            })
            .ToListAsync();

        return new ArchiveListDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}



