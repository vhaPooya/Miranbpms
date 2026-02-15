using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر برای دریافت داده‌های Metadata (انواع مدرک، طبقه‌بندی، و غیره)
/// </summary>
public class MetadataController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly ICacheService? _cacheService;
    private readonly ILogger<MetadataController> _logger;

    public MetadataController(AutomationDbContext context, ILogger<MetadataController> logger, ICacheService? cacheService = null)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// دریافت لیست انواع مدرک
    /// </summary>
    [HttpGet]
    [Route("api/metadata/document-types")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        try
        {
            var cacheKey = "DocumentTypes_All";
            var documentTypes = _cacheService?.GetOrSet(
                cacheKey,
                () => _context.DocumentTypes
                    .Where(dt => !dt.IsDeleted)
                    .OrderBy(dt => dt.DisplayOrder)
                    .ThenBy(dt => dt.DocumentTypeName)
                    .Select(dt => new
                    {
                        dt.Id,
                        dt.DocumentTypeCode,
                        dt.DocumentTypeName,
                        dt.Description,
                        dt.DisplayOrder
                    })
                    .ToList(),
                TimeSpan.FromHours(24)
            );

            if (documentTypes == null)
            {
                documentTypes = await _context.DocumentTypes
                    .Where(dt => !dt.IsDeleted)
                    .OrderBy(dt => dt.DisplayOrder)
                    .ThenBy(dt => dt.DocumentTypeName)
                    .Select(dt => new
                    {
                        dt.Id,
                        dt.DocumentTypeCode,
                        dt.DocumentTypeName,
                        dt.Description,
                        dt.DisplayOrder
                    })
                    .ToListAsync();
            }

            return Json(new { success = true, data = documentTypes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document types");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست طبقه‌بندی‌ها
    /// </summary>
    [HttpGet]
    [Route("api/metadata/classifications")]
    public async Task<IActionResult> GetClassifications()
    {
        try
        {
            var cacheKey = "Classifications_All";
            var classifications = _cacheService?.GetOrSet(
                cacheKey,
                () => _context.Classifications
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.ClassificationName)
                    .Select(c => new
                    {
                        c.Id,
                        c.ClassificationCode,
                        c.ClassificationName,
                        c.Description,
                        c.DisplayOrder
                    })
                    .ToList(),
                TimeSpan.FromHours(24)
            );

            if (classifications == null)
            {
                classifications = await _context.Classifications
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.ClassificationName)
                    .Select(c => new
                    {
                        c.Id,
                        c.ClassificationCode,
                        c.ClassificationName,
                        c.Description,
                        c.DisplayOrder
                    })
                    .ToListAsync();
            }

            return Json(new { success = true, data = classifications });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting classifications");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست نحوه دریافت
    /// </summary>
    [HttpGet]
    [Route("api/metadata/receipt-methods")]
    public async Task<IActionResult> GetReceiptMethods()
    {
        try
        {
            var cacheKey = "ReceiptMethods_All";
            var receiptMethods = _cacheService?.GetOrSet(
                cacheKey,
                () => _context.ReceiptMethods
                    .Where(rm => !rm.IsDeleted)
                    .OrderBy(rm => rm.DisplayOrder)
                    .ThenBy(rm => rm.ReceiptMethodName)
                    .Select(rm => new
                    {
                        rm.Id,
                        rm.ReceiptMethodCode,
                        rm.ReceiptMethodName,
                        rm.Description,
                        rm.DisplayOrder
                    })
                    .ToList(),
                TimeSpan.FromHours(24)
            );

            if (receiptMethods == null)
            {
                receiptMethods = await _context.ReceiptMethods
                    .Where(rm => !rm.IsDeleted)
                    .OrderBy(rm => rm.DisplayOrder)
                    .ThenBy(rm => rm.ReceiptMethodName)
                    .Select(rm => new
                    {
                        rm.Id,
                        rm.ReceiptMethodCode,
                        rm.ReceiptMethodName,
                        rm.Description,
                        rm.DisplayOrder
                    })
                    .ToListAsync();
            }

            return Json(new { success = true, data = receiptMethods });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt methods");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست فوریت‌ها
    /// </summary>
    [HttpGet]
    [Route("api/metadata/urgencies")]
    public async Task<IActionResult> GetUrgencies()
    {
        try
        {
            var cacheKey = "Urgencies_All";
            var urgencies = _cacheService?.GetOrSet(
                cacheKey,
                () => _context.Urgencies
                    .Where(u => !u.IsDeleted)
                    .OrderBy(u => u.DisplayOrder)
                    .ThenBy(u => u.UrgencyName)
                    .Select(u => new
                    {
                        u.Id,
                        u.UrgencyCode,
                        u.UrgencyName,
                        u.Description,
                        u.DisplayOrder
                    })
                    .ToList(),
                TimeSpan.FromHours(24)
            );

            if (urgencies == null)
            {
                urgencies = await _context.Urgencies
                    .Where(u => !u.IsDeleted)
                    .OrderBy(u => u.DisplayOrder)
                    .ThenBy(u => u.UrgencyName)
                    .Select(u => new
                    {
                        u.Id,
                        u.UrgencyCode,
                        u.UrgencyName,
                        u.Description,
                        u.DisplayOrder
                    })
                    .ToListAsync();
            }

            return Json(new { success = true, data = urgencies });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting urgencies");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// صفحه مدیریت پارامترهای دبیرخانه
    /// </summary>
    [HttpGet]
    [Route("metadata")]
    public IActionResult Index()
    {
        return View();
    }

    // ==========================================
    //           DOCUMENT TYPES CRUD
    // ==========================================

    [HttpPost]
    [Route("api/metadata/document-types")]
    public async Task<IActionResult> CreateDocumentType([FromBody] CreateDocumentTypeDto dto)
    {
        try
        {
            if (await _context.DocumentTypes.AnyAsync(dt => dt.DocumentTypeCode == dto.DocumentTypeCode && !dt.IsDeleted))
                return Json(new { success = false, error = "کد نوع مدرک تکراری است" });

            var docType = new DocumentType
            {
                DocumentTypeCode = dto.DocumentTypeCode,
                DocumentTypeName = dto.DocumentTypeName,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder ?? 0
            };

            _context.DocumentTypes.Add(docType);
            await _context.SaveChangesAsync();
            _cacheService?.Remove("DocumentTypes_All");

            return Json(new { success = true, id = docType.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document type");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPut]
    [Route("api/metadata/document-types/{id}")]
    public async Task<IActionResult> UpdateDocumentType(int id, [FromBody] UpdateDocumentTypeDto dto)
    {
        try
        {
            var docType = await _context.DocumentTypes.FindAsync(id);
            if (docType == null || docType.IsDeleted)
                return Json(new { success = false, error = "نوع مدرک یافت نشد" });

            if (dto.DocumentTypeCode != docType.DocumentTypeCode && 
                await _context.DocumentTypes.AnyAsync(dt => dt.DocumentTypeCode == dto.DocumentTypeCode && dt.Id != id && !dt.IsDeleted))
                return Json(new { success = false, error = "کد نوع مدرک تکراری است" });

            docType.DocumentTypeCode = dto.DocumentTypeCode;
            docType.DocumentTypeName = dto.DocumentTypeName;
            docType.Description = dto.Description;
            if (dto.DisplayOrder.HasValue) docType.DisplayOrder = dto.DisplayOrder.Value;
            docType.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cacheService?.Remove("DocumentTypes_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document type");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete]
    [Route("api/metadata/document-types/{id}")]
    public async Task<IActionResult> DeleteDocumentType(int id)
    {
        try
        {
            var docType = await _context.DocumentTypes.FindAsync(id);
            if (docType == null || docType.IsDeleted)
                return Json(new { success = false, error = "نوع مدرک یافت نشد" });

            // بررسی استفاده در مدارک
            if (await _context.IncomingDocuments.AnyAsync(d => d.DocumentTypeId == id && !d.IsDeleted) ||
                await _context.OutgoingDocuments.AnyAsync(d => d.DocumentTypeId == id && !d.IsDeleted))
                return Json(new { success = false, error = "نمی‌توان نوع مدرکی که در مدارک استفاده شده است را حذف کرد" });

            docType.IsDeleted = true;
            docType.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cacheService?.Remove("DocumentTypes_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document type");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ==========================================
    //           CLASSIFICATIONS CRUD
    // ==========================================

    [HttpPost]
    [Route("api/metadata/classifications")]
    public async Task<IActionResult> CreateClassification([FromBody] CreateClassificationDto dto)
    {
        try
        {
            if (await _context.Classifications.AnyAsync(c => c.ClassificationCode == dto.ClassificationCode && !c.IsDeleted))
                return Json(new { success = false, error = "کد طبقه‌بندی تکراری است" });

            var classification = new Classification
            {
                ClassificationCode = dto.ClassificationCode,
                ClassificationName = dto.ClassificationName,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder ?? 0
            };

            _context.Classifications.Add(classification);
            await _context.SaveChangesAsync();
            _cacheService?.Remove("Classifications_All");

            return Json(new { success = true, id = classification.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating classification");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPut]
    [Route("api/metadata/classifications/{id}")]
    public async Task<IActionResult> UpdateClassification(int id, [FromBody] UpdateClassificationDto dto)
    {
        try
        {
            var classification = await _context.Classifications.FindAsync(id);
            if (classification == null || classification.IsDeleted)
                return Json(new { success = false, error = "طبقه‌بندی یافت نشد" });

            if (dto.ClassificationCode != classification.ClassificationCode && 
                await _context.Classifications.AnyAsync(c => c.ClassificationCode == dto.ClassificationCode && c.Id != id && !c.IsDeleted))
                return Json(new { success = false, error = "کد طبقه‌بندی تکراری است" });

            classification.ClassificationCode = dto.ClassificationCode;
            classification.ClassificationName = dto.ClassificationName;
            classification.Description = dto.Description;
            if (dto.DisplayOrder.HasValue) classification.DisplayOrder = dto.DisplayOrder.Value;
            classification.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cacheService?.Remove("Classifications_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating classification");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete]
    [Route("api/metadata/classifications/{id}")]
    public async Task<IActionResult> DeleteClassification(int id)
    {
        try
        {
            var classification = await _context.Classifications.FindAsync(id);
            if (classification == null || classification.IsDeleted)
                return Json(new { success = false, error = "طبقه‌بندی یافت نشد" });

            if (await _context.IncomingDocuments.AnyAsync(d => d.ClassificationId == id && !d.IsDeleted) ||
                await _context.OutgoingDocuments.AnyAsync(d => d.ClassificationId == id && !d.IsDeleted))
                return Json(new { success = false, error = "نمی‌توان طبقه‌بندی که در مدارک استفاده شده است را حذف کرد" });

            classification.IsDeleted = true;
            classification.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cacheService?.Remove("Classifications_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting classification");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ==========================================
    //           RECEIPT METHODS CRUD
    // ==========================================

    [HttpPost]
    [Route("api/metadata/receipt-methods")]
    public async Task<IActionResult> CreateReceiptMethod([FromBody] CreateReceiptMethodDto dto)
    {
        try
        {
            if (await _context.ReceiptMethods.AnyAsync(rm => rm.ReceiptMethodCode == dto.ReceiptMethodCode && !rm.IsDeleted))
                return Json(new { success = false, error = "کد نحوه دریافت تکراری است" });

            var receiptMethod = new ReceiptMethod
            {
                ReceiptMethodCode = dto.ReceiptMethodCode,
                ReceiptMethodName = dto.ReceiptMethodName,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder ?? 0
            };

            _context.ReceiptMethods.Add(receiptMethod);
            await _context.SaveChangesAsync();
            _cacheService?.Remove("ReceiptMethods_All");

            return Json(new { success = true, id = receiptMethod.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receipt method");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPut]
    [Route("api/metadata/receipt-methods/{id}")]
    public async Task<IActionResult> UpdateReceiptMethod(int id, [FromBody] UpdateReceiptMethodDto dto)
    {
        try
        {
            var receiptMethod = await _context.ReceiptMethods.FindAsync(id);
            if (receiptMethod == null || receiptMethod.IsDeleted)
                return Json(new { success = false, error = "نحوه دریافت یافت نشد" });

            if (dto.ReceiptMethodCode != receiptMethod.ReceiptMethodCode && 
                await _context.ReceiptMethods.AnyAsync(rm => rm.ReceiptMethodCode == dto.ReceiptMethodCode && rm.Id != id && !rm.IsDeleted))
                return Json(new { success = false, error = "کد نحوه دریافت تکراری است" });

            receiptMethod.ReceiptMethodCode = dto.ReceiptMethodCode;
            receiptMethod.ReceiptMethodName = dto.ReceiptMethodName;
            receiptMethod.Description = dto.Description;
            if (dto.DisplayOrder.HasValue) receiptMethod.DisplayOrder = dto.DisplayOrder.Value;
            receiptMethod.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cacheService?.Remove("ReceiptMethods_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt method");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete]
    [Route("api/metadata/receipt-methods/{id}")]
    public async Task<IActionResult> DeleteReceiptMethod(int id)
    {
        try
        {
            var receiptMethod = await _context.ReceiptMethods.FindAsync(id);
            if (receiptMethod == null || receiptMethod.IsDeleted)
                return Json(new { success = false, error = "نحوه دریافت یافت نشد" });

            if (await _context.IncomingDocuments.AnyAsync(d => d.ReceiptMethodId == id && !d.IsDeleted))
                return Json(new { success = false, error = "نمی‌توان نحوه دریافت که در مدارک استفاده شده است را حذف کرد" });

            receiptMethod.IsDeleted = true;
            receiptMethod.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cacheService?.Remove("ReceiptMethods_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt method");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ==========================================
    //           URGENCIES CRUD
    // ==========================================

    [HttpPost]
    [Route("api/metadata/urgencies")]
    public async Task<IActionResult> CreateUrgency([FromBody] CreateUrgencyDto dto)
    {
        try
        {
            if (await _context.Urgencies.AnyAsync(u => u.UrgencyCode == dto.UrgencyCode && !u.IsDeleted))
                return Json(new { success = false, error = "کد فوریت تکراری است" });

            var urgency = new Urgency
            {
                UrgencyCode = dto.UrgencyCode,
                UrgencyName = dto.UrgencyName,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder ?? 0
            };

            _context.Urgencies.Add(urgency);
            await _context.SaveChangesAsync();
            _cacheService?.Remove("Urgencies_All");

            return Json(new { success = true, id = urgency.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating urgency");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPut]
    [Route("api/metadata/urgencies/{id}")]
    public async Task<IActionResult> UpdateUrgency(int id, [FromBody] UpdateUrgencyDto dto)
    {
        try
        {
            var urgency = await _context.Urgencies.FindAsync(id);
            if (urgency == null || urgency.IsDeleted)
                return Json(new { success = false, error = "فوریت یافت نشد" });

            if (dto.UrgencyCode != urgency.UrgencyCode && 
                await _context.Urgencies.AnyAsync(u => u.UrgencyCode == dto.UrgencyCode && u.Id != id && !u.IsDeleted))
                return Json(new { success = false, error = "کد فوریت تکراری است" });

            urgency.UrgencyCode = dto.UrgencyCode;
            urgency.UrgencyName = dto.UrgencyName;
            urgency.Description = dto.Description;
            if (dto.DisplayOrder.HasValue) urgency.DisplayOrder = dto.DisplayOrder.Value;
            urgency.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cacheService?.Remove("Urgencies_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating urgency");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete]
    [Route("api/metadata/urgencies/{id}")]
    public async Task<IActionResult> DeleteUrgency(int id)
    {
        try
        {
            var urgency = await _context.Urgencies.FindAsync(id);
            if (urgency == null || urgency.IsDeleted)
                return Json(new { success = false, error = "فوریت یافت نشد" });

            if (await _context.IncomingDocuments.AnyAsync(d => d.UrgencyId == id && !d.IsDeleted) ||
                await _context.OutgoingDocuments.AnyAsync(d => d.UrgencyId == id && !d.IsDeleted))
                return Json(new { success = false, error = "نمی‌توان فوریتی که در مدارک استفاده شده است را حذف کرد" });

            urgency.IsDeleted = true;
            urgency.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cacheService?.Remove("Urgencies_All");

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting urgency");
            return Json(new { success = false, error = ex.Message });
        }
    }
}




