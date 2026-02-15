using Automation.Core.DTOs;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس جستجوی پیشرفته
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// جستجوی سراسری
    /// </summary>
    Task<SearchResultDto> GlobalSearchAsync(GlobalSearchDto searchDto);
    
    /// <summary>
    /// جستجو در فرم‌ها
    /// </summary>
    Task<SearchResultDto> SearchFormsAsync(FormSearchDto searchDto);
    
    /// <summary>
    /// جستجو در مدارک
    /// </summary>
    Task<SearchResultDto> SearchDocumentsAsync(DocumentSearchDto searchDto);
    
    /// <summary>
    /// جستجوی پیشرفته با فیلترهای متعدد
    /// </summary>
    Task<SearchResultDto> AdvancedSearchAsync(AdvancedSearchDto searchDto);
}




