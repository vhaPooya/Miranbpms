using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس جستجوی پیشرفته
/// </summary>
public class SearchService : ISearchService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<SearchService> _logger;

    public SearchService(AutomationDbContext context, ILogger<SearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SearchResultDto> GlobalSearchAsync(GlobalSearchDto searchDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var query = _context.Documents
                .Include(d => d.Form)
                .Include(d => d.CreatedByUser)
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(searchDto.Query))
            {
                query = query.Where(d =>
                    d.DocumentNumber.Contains(searchDto.Query) ||
                    d.Subject.Contains(searchDto.Query) ||
                    (d.Content != null && d.Content.Contains(searchDto.Query)));
            }

            if (searchDto.DocumentTypes != null && searchDto.DocumentTypes.Any())
            {
                query = query.Where(d => searchDto.DocumentTypes.Contains(d.DocumentType));
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(d => d.CreatedDateTime >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(d => d.CreatedDateTime <= searchDto.ToDate.Value);
            }

            if (searchDto.FormId.HasValue)
            {
                query = query.Where(d => d.FormId == searchDto.FormId.Value);
            }

            var total = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.CreatedDateTime)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            // دریافت شماره‌های وارده/صادره
            var documentIds = documents.Select(d => d.Id).ToList();
            var incomingNumbers = await _context.IncomingDocuments
                .Where(id => documentIds.Contains(id.DocumentId))
                .ToDictionaryAsync(id => id.DocumentId, id => id.IncomingNumber);

            var outgoingNumbers = await _context.OutgoingDocuments
                .Where(od => documentIds.Contains(od.DocumentId))
                .ToDictionaryAsync(od => od.DocumentId, od => od.OutgoingNumber);

            var items = documents.Select(d =>
            {
                var snippet = GetSnippet(d.Content, searchDto.Query, 200);
                
                return new SearchResultItemDto
                {
                    DocumentId = d.Id,
                    DocumentNumber = d.DocumentNumber,
                    IncomingNumber = incomingNumbers.GetValueOrDefault(d.Id),
                    OutgoingNumber = outgoingNumbers.GetValueOrDefault(d.Id),
                    Subject = d.Subject,
                    DocumentType = d.DocumentType,
                    Status = d.Status,
                    CreatedDateTime = d.CreatedDateTime,
                    CreatedByUserName = d.CreatedByUser?.FullName,
                    FormName = d.Form?.NameFa,
                    Snippet = snippet,
                    RelevanceScore = CalculateRelevanceScore(d, searchDto.Query)
                };
            }).ToList();

            stopwatch.Stop();

            return new SearchResultDto
            {
                Items = items,
                Total = total,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                SearchDuration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in global search");
            return new SearchResultDto { Items = new List<SearchResultItemDto>(), Total = 0, Page = searchDto.Page, PageSize = searchDto.PageSize, SearchDuration = TimeSpan.Zero };
        }
    }

    public async Task<SearchResultDto> SearchFormsAsync(FormSearchDto searchDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // TODO: جستجو در داده‌های فرم‌ها (از جداول داینامیک)
            // در حال حاضر فقط جستجو در Documents
            var globalSearch = new GlobalSearchDto
            {
                Query = searchDto.Query ?? "",
                FormId = searchDto.FormId,
                FromDate = searchDto.FromDate,
                ToDate = searchDto.ToDate,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };

            return await GlobalSearchAsync(globalSearch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in form search");
            return new SearchResultDto { Items = new List<SearchResultItemDto>(), Total = 0, Page = searchDto.Page, PageSize = searchDto.PageSize, SearchDuration = TimeSpan.Zero };
        }
    }

    public async Task<SearchResultDto> SearchDocumentsAsync(DocumentSearchDto searchDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var query = _context.Documents
                .Include(d => d.Form)
                .Include(d => d.CreatedByUser)
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(searchDto.DocumentNumber))
            {
                query = query.Where(d => d.DocumentNumber.Contains(searchDto.DocumentNumber));
            }

            if (!string.IsNullOrEmpty(searchDto.Subject))
            {
                query = query.Where(d => d.Subject.Contains(searchDto.Subject));
            }

            if (!string.IsNullOrEmpty(searchDto.Content))
            {
                query = query.Where(d => d.Content != null && d.Content.Contains(searchDto.Content));
            }

            if (searchDto.DocumentTypes != null && searchDto.DocumentTypes.Any())
            {
                query = query.Where(d => searchDto.DocumentTypes.Contains(d.DocumentType));
            }

            if (searchDto.Statuses != null && searchDto.Statuses.Any())
            {
                query = query.Where(d => searchDto.Statuses.Contains(d.Status));
            }

            if (searchDto.CreatedByUserId.HasValue)
            {
                query = query.Where(d => d.CreatedByUserId == searchDto.CreatedByUserId.Value);
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(d => d.CreatedDateTime >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(d => d.CreatedDateTime <= searchDto.ToDate.Value);
            }

            if (searchDto.ConfidentialityLevelId.HasValue)
            {
                query = query.Where(d => d.ConfidentialityLevelId == searchDto.ConfidentialityLevelId.Value);
            }

            if (searchDto.PriorityId.HasValue)
            {
                query = query.Where(d => d.PriorityId == searchDto.PriorityId.Value);
            }

            var total = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.CreatedDateTime)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var documentIds = documents.Select(d => d.Id).ToList();
            var incomingNumbers = await _context.IncomingDocuments
                .Where(id => documentIds.Contains(id.DocumentId))
                .ToDictionaryAsync(id => id.DocumentId, id => id.IncomingNumber);

            var outgoingNumbers = await _context.OutgoingDocuments
                .Where(od => documentIds.Contains(od.DocumentId))
                .ToDictionaryAsync(od => od.DocumentId, od => od.OutgoingNumber);

            var items = documents.Select(d => new SearchResultItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                IncomingNumber = incomingNumbers.GetValueOrDefault(d.Id),
                OutgoingNumber = outgoingNumbers.GetValueOrDefault(d.Id),
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                Status = d.Status,
                CreatedDateTime = d.CreatedDateTime,
                CreatedByUserName = d.CreatedByUser?.FullName,
                FormName = d.Form?.NameFa,
                Snippet = GetSnippet(d.Content, searchDto.Content ?? searchDto.Subject ?? "", 200)
            }).ToList();

            stopwatch.Stop();

            return new SearchResultDto
            {
                Items = items,
                Total = total,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                SearchDuration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in document search");
            return new SearchResultDto { Items = new List<SearchResultItemDto>(), Total = 0, Page = searchDto.Page, PageSize = searchDto.PageSize, SearchDuration = TimeSpan.Zero };
        }
    }

    public async Task<SearchResultDto> AdvancedSearchAsync(AdvancedSearchDto searchDto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var query = _context.Documents
                .Include(d => d.Form)
                .Include(d => d.CreatedByUser)
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(searchDto.Query))
            {
                query = query.Where(d =>
                    d.DocumentNumber.Contains(searchDto.Query) ||
                    d.Subject.Contains(searchDto.Query) ||
                    (d.Content != null && d.Content.Contains(searchDto.Query)));
            }

            if (searchDto.FormId.HasValue)
            {
                query = query.Where(d => d.FormId == searchDto.FormId.Value);
            }

            if (searchDto.DocumentTypes != null && searchDto.DocumentTypes.Any())
            {
                query = query.Where(d => searchDto.DocumentTypes.Contains(d.DocumentType));
            }

            if (searchDto.Statuses != null && searchDto.Statuses.Any())
            {
                query = query.Where(d => searchDto.Statuses.Contains(d.Status));
            }

            if (searchDto.CreatedByUserId.HasValue)
            {
                query = query.Where(d => d.CreatedByUserId == searchDto.CreatedByUserId.Value);
            }

            if (searchDto.OrganizationId.HasValue)
            {
                query = query.Where(d => d.CreatedByUser.OrganizationId == searchDto.OrganizationId.Value);
            }

            if (searchDto.DepartmentId.HasValue)
            {
                query = query.Where(d => d.CreatedByUser.DepartmentId == searchDto.DepartmentId.Value);
            }

            if (searchDto.FromDate.HasValue)
            {
                query = query.Where(d => d.CreatedDateTime >= searchDto.FromDate.Value);
            }

            if (searchDto.ToDate.HasValue)
            {
                query = query.Where(d => d.CreatedDateTime <= searchDto.ToDate.Value);
            }

            if (searchDto.IsSigned.HasValue)
            {
                query = query.Where(d => d.IsSigned == searchDto.IsSigned.Value);
            }

            if (searchDto.IsArchived.HasValue)
            {
                query = query.Where(d => d.IsArchived == searchDto.IsArchived.Value);
            }

            if (searchDto.ConfidentialityLevelId.HasValue)
            {
                query = query.Where(d => d.ConfidentialityLevelId == searchDto.ConfidentialityLevelId.Value);
            }

            if (searchDto.PriorityId.HasValue)
            {
                query = query.Where(d => d.PriorityId == searchDto.PriorityId.Value);
            }

            var total = await query.CountAsync();

            var documents = await query
                .OrderByDescending(d => d.CreatedDateTime)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var documentIds = documents.Select(d => d.Id).ToList();
            var incomingNumbers = await _context.IncomingDocuments
                .Where(id => documentIds.Contains(id.DocumentId))
                .ToDictionaryAsync(id => id.DocumentId, id => id.IncomingNumber);

            var outgoingNumbers = await _context.OutgoingDocuments
                .Where(od => documentIds.Contains(od.DocumentId))
                .ToDictionaryAsync(od => od.DocumentId, od => od.OutgoingNumber);

            var items = documents.Select(d => new SearchResultItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                IncomingNumber = incomingNumbers.GetValueOrDefault(d.Id),
                OutgoingNumber = outgoingNumbers.GetValueOrDefault(d.Id),
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                Status = d.Status,
                CreatedDateTime = d.CreatedDateTime,
                CreatedByUserName = d.CreatedByUser?.FullName,
                FormName = d.Form?.NameFa,
                Snippet = GetSnippet(d.Content, searchDto.Query ?? "", 200),
                RelevanceScore = CalculateRelevanceScore(d, searchDto.Query ?? "")
            }).ToList();

            stopwatch.Stop();

            return new SearchResultDto
            {
                Items = items,
                Total = total,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                SearchDuration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced search");
            return new SearchResultDto { Items = new List<SearchResultItemDto>(), Total = 0, Page = searchDto.Page, PageSize = searchDto.PageSize, SearchDuration = TimeSpan.Zero };
        }
    }

    // Helper Methods
    private string? GetSnippet(string? content, string query, int maxLength)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(query))
            return null;

        var index = content.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            return content.Length > maxLength ? content.Substring(0, maxLength) + "..." : content;
        }

        var start = Math.Max(0, index - 50);
        var end = Math.Min(content.Length, index + query.Length + 50);
        var snippet = content.Substring(start, end - start);

        if (start > 0) snippet = "..." + snippet;
        if (end < content.Length) snippet = snippet + "...";

        return snippet;
    }

    private double? CalculateRelevanceScore(Document document, string query)
    {
        if (string.IsNullOrEmpty(query))
            return null;

        double score = 0;

        // امتیاز برای تطابق در شماره مدرک
        if (document.DocumentNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
            score += 10;

        // امتیاز برای تطابق در موضوع
        if (document.Subject.Contains(query, StringComparison.OrdinalIgnoreCase))
            score += 5;

        // امتیاز برای تطابق در محتوا
        if (!string.IsNullOrEmpty(document.Content) && document.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            score += 2;

        return score;
    }
}




