using Automation.Core.DTOs;
using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر جستجوی پیشرفته
/// </summary>
public class SearchController : Controller
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه جستجو
    /// </summary>
    [HttpGet]
    [Route("search")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    /// <summary>
    /// جستجوی سراسری
    /// </summary>
    [HttpPost]
    [Route("api/search/global")]
    public async Task<IActionResult> GlobalSearch([FromBody] GlobalSearchDto searchDto)
    {
        try
        {
            var result = await _searchService.GlobalSearchAsync(searchDto);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in global search");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// جستجو در فرم‌ها
    /// </summary>
    [HttpPost]
    [Route("api/search/forms")]
    public async Task<IActionResult> SearchForms([FromBody] FormSearchDto searchDto)
    {
        try
        {
            var result = await _searchService.SearchFormsAsync(searchDto);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in form search");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// جستجو در مدارک
    /// </summary>
    [HttpPost]
    [Route("api/search/documents")]
    public async Task<IActionResult> SearchDocuments([FromBody] DocumentSearchDto searchDto)
    {
        try
        {
            var result = await _searchService.SearchDocumentsAsync(searchDto);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in document search");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// جستجوی پیشرفته
    /// </summary>
    [HttpPost]
    [Route("api/search/advanced")]
    public async Task<IActionResult> AdvancedSearch([FromBody] AdvancedSearchDto searchDto)
    {
        try
        {
            var result = await _searchService.AdvancedSearchAsync(searchDto);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced search");
            return Json(new { success = false, error = ex.Message });
        }
    }
}




