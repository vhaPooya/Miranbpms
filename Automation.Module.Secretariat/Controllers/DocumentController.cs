using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Module.Secretariat.Controllers;

/// <summary>
/// کنترلر مدیریت اسناد (Document Management)
/// </summary>
public class DocumentController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly CoreDbContext _coreContext;
    private readonly IDocumentService _documentService;
    private readonly IPermissionService _permissionService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        AutomationDbContext context,
        CoreDbContext coreContext,
        IDocumentService documentService,
        IPermissionService permissionService,
        IUserContextService userContextService,
        ILogger<DocumentController> logger)
    {
        _context = context;
        _coreContext = coreContext;
        _documentService = documentService;
        _permissionService = permissionService;
        _userContextService = userContextService;
        _logger = logger;
    }

    private int GetCurrentUserId() => _userContextService.GetCurrentUserId() ?? 0;

    [HttpPost]
    [Route("api/documents/{documentId}/attachments")]
    public async Task<IActionResult> UploadAttachment(int documentId, IFormFile file, [FromForm] string? description = null, [FromForm] bool isReference = false)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, error = "فایل انتخاب نشده است" });

            Document? document = null;
            if (documentId != 0)
            {
                document = await _context.Documents
                    .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);
            }

            if (document == null)
            {
                var formId = int.Parse(Request.Form["formId"].ToString());
                document = new Document
                {
                    FormId = formId,
                    DocumentNumber = $"TEMP-{Guid.NewGuid()}",
                    DocumentType = "FORM_DATA",
                    Subject = "سند موقت",
                    Content = "",
                    CreatedByUserId = userId,
                    CreatedDateTime = DateTime.UtcNow,
                    Status = "DRAFT"
                };
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "attachments");
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var result = await _documentService.AttachExternalFileAsync(
                document.Id,
                document.FormId,
                file.FileName,
                filePath,
                file.Length,
                file.ContentType ?? "application/octet-stream",
                userId,
                description
            );

            if (!result)
            {
                return Json(new { success = false, error = "خطا در ذخیره پیوست" });
            }

            var createdAttachment = await _context.DocumentAttachments
                .Where(a => a.DocumentId == document.Id && a.UploadedByUserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    id = createdAttachment?.Id ?? 0,
                    fileName = file.FileName,
                    fileSize = file.Length,
                    description = description,
                    isReference = isReference
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for document {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/documents/{documentId}/attachments")]
    public async Task<IActionResult> GetAttachments(int documentId, [FromQuery] bool? isReference = null)
    {
        try
        {
            var attachments = await _documentService.GetDocumentAttachmentsAsync(documentId);

            if (isReference.HasValue)
            {
                attachments = attachments.Where(a => a.AttachmentCategory == (isReference.Value ? "REFERENCE" : "ATTACHMENT")).ToList();
            }

            return Json(new
            {
                success = true,
                data = attachments.Select(a => new
                {
                    id = a.Id,
                    fileName = a.FileName,
                    fileSize = a.FileSize,
                    mimeType = a.MimeType,
                    description = a.Description,
                    isReference = a.AttachmentCategory == "REFERENCE",
                    uploadedDate = a.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attachments for document {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/documents/attachments/{attachmentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        try
        {
            var attachment = await _context.DocumentAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);

            if (attachment == null)
                return NotFound();

            var filePath = attachment.FilePath;

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, attachment.MimeType, attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete]
    [Route("api/documents/attachments/{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });

            var result = await _documentService.DeleteAttachmentAsync(attachmentId, userId);
            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [Route("api/documents/{documentId}/refer")]
    public async Task<IActionResult> ReferDocument(int documentId, [FromBody] ReferDocumentDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });

            var document = await _context.Documents
                .Include(d => d.Form)
                .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

            if (document == null)
                return NotFound(new { success = false, error = "سند یافت نشد" });

            var success = false;
            var referralIds = new List<int>();
            if (dto.ReferredToUserIds != null && dto.ReferredToUserIds.Any())
            {
                foreach (var referredUserId in dto.ReferredToUserIds)
                {
                    success = await _documentService.ReferDocumentAsync(
                        documentId,
                        dto.FormId,
                        dto.ActionTypeId,
                        userId,
                        referredUserId,
                        null,
                        null,
                        null,
                        dto.Notes,
                        dto.DueDate
                    );
                    if (success)
                    {
                        var referral = await _context.DocumentReferrals
                            .Where(r => r.DocumentId == documentId && r.ReferredToUserId == referredUserId && r.ReferredByUserId == userId)
                            .OrderByDescending(r => r.CreatedAt)
                            .FirstOrDefaultAsync();
                        if (referral != null)
                            referralIds.Add(referral.Id);
                    }
                }
            }
            if (dto.ReferredToRoleIds != null && dto.ReferredToRoleIds.Any())
            {
                foreach (var roleId in dto.ReferredToRoleIds)
                {
                    success = await _documentService.ReferDocumentAsync(
                        documentId,
                        dto.FormId,
                        dto.ActionTypeId,
                        userId,
                        null,
                        roleId,
                        null,
                        null,
                        dto.Notes,
                        dto.DueDate
                    );
                }
            }
            if (dto.ReferredToDepartmentIds != null && dto.ReferredToDepartmentIds.Any())
            {
                foreach (var deptId in dto.ReferredToDepartmentIds)
                {
                    success = await _documentService.ReferDocumentAsync(
                        documentId,
                        dto.FormId,
                        dto.ActionTypeId,
                        userId,
                        null,
                        null,
                        deptId,
                        null,
                        dto.Notes,
                        dto.DueDate
                    );
                }
            }
            if (dto.ReferredToGroupIds != null && dto.ReferredToGroupIds.Any())
            {
                foreach (var groupId in dto.ReferredToGroupIds)
                {
                    success = await _documentService.ReferDocumentAsync(
                        documentId,
                        dto.FormId,
                        dto.ActionTypeId,
                        userId,
                        null,
                        null,
                        null,
                        groupId,
                        dto.Notes,
                        dto.DueDate
                    );
                }
            }

            var actionType = await _coreContext.ActionTypes.FindAsync(dto.ActionTypeId);
            if (actionType != null && !actionType.IsEditable)
            {
                document.Status = "LOCKED";
                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                success = success,
                data = new
                {
                    referralIds = referralIds,
                    status = document.Status
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error referring document {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [Route("api/documents/{documentId}/sign")]
    public async Task<IActionResult> SignDocument(int documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

            if (document == null)
                return NotFound(new { success = false, error = "سند یافت نشد" });

            if (document.IsSigned)
                return BadRequest(new { success = false, error = "این سند قبلا امضا شده است" });

            document.IsSigned = true;
            document.SignedDateTime = DateTime.UtcNow;
            document.SignedByUserId = userId;
            document.Status = "SIGNED";
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing document {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/documents/{documentId}/can-edit")]
    public async Task<IActionResult> CanEditDocument(int documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, canEdit = false });

            var canEdit = await _permissionService.CanEditDocumentAsync(userId, documentId);
            return Json(new { success = true, canEdit = canEdit });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permission for document {DocumentId}", documentId);
            return Json(new { success = false, canEdit = false });
        }
    }
}
