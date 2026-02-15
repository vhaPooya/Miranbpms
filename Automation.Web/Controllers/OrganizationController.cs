using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت سازمان‌ها
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OrganizationController : ControllerBase
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(IdentityDbContext context, ILogger<OrganizationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست سازمان‌ها
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrganizations()
    {
        try
        {
            var organizations = await _context.Organizations
                .Where(o => !o.IsDeleted)
                .OrderBy(o => o.OrganizationName)
                .Select(o => new
                {
                    o.Id,
                    o.OrganizationCode,
                    o.OrganizationName,
                    o.Description,
                    o.IsActive
                })
                .ToListAsync();

            return Ok(new { success = true, data = organizations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizations");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت سازمان بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrganization(int id)
    {
        try
        {
            var organization = await _context.Organizations
                .Where(o => o.Id == id && !o.IsDeleted)
                .Select(o => new
                {
                    o.Id,
                    o.OrganizationCode,
                    o.OrganizationName,
                    o.Description,
                    o.IsActive
                })
                .FirstOrDefaultAsync();

            if (organization == null)
            {
                return NotFound(new { success = false, error = "سازمان یافت نشد" });
            }

            return Ok(new { success = true, data = organization });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}




