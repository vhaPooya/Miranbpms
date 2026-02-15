using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Automation.Core.Interfaces;
using Automation.Module.Workflow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Module.Workflow.Controllers;

[Route("workflow")]
public class WorkflowController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly IUserContextService _userContextService;

    public WorkflowController(AutomationDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    /// <summary>
    /// صفحه اصلی فرآیندساز (طراحی فرآیند)
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("visualize/{documentId}")]
    public async Task<IActionResult> Visualize(int documentId)
    {
        var document = await _context.Set<Document>()
            .Include(d => d.DocumentTrackings)
                .ThenInclude(t => t.User)
            .Include(d => d.DocumentTrackings)
                .ThenInclude(t => t.Position)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
        {
            return NotFound();
        }

        var viewModel = new WorkflowVisualizationDto
        {
            DocumentId = document.Id,
            DocumentNumber = document.DocumentNumber,
            Subject = document.Subject,
            Trackings = document.DocumentTrackings.Select(t => new TrackingNodeDto
            {
                Id = t.Id,
                UserId = t.UserId,
                PositionId = t.PositionId,
                UserName = t.User?.FirstName + " " + t.User?.LastName,
                PositionName = t.Position?.PositionName,
                ReceivedDate = t.ReceivedDate,
                FirstViewDate = t.FirstViewDate,
                CompletionDate = t.CompletionDate,
                Status = t.Status,
                Notes = t.Notes
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet("diagram/{documentId}")]
    public async Task<IActionResult> Diagram(int documentId)
    {
        var document = await _context.Set<Document>()
            .Include(d => d.DocumentTrackings)
                .ThenInclude(t => t.User)
            .Include(d => d.DocumentTrackings)
                .ThenInclude(t => t.Position)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
        {
            return NotFound();
        }

        var viewModel = new WorkflowDiagramDto
        {
            DocumentId = document.Id,
            DocumentNumber = document.DocumentNumber,
            Subject = document.Subject,
            Nodes = document.DocumentTrackings.Select(t => new DiagramNodeDto
            {
                Id = t.Id,
                UserId = t.UserId,
                PositionId = t.PositionId,
                UserName = t.User?.FirstName + " " + t.User?.LastName,
                PositionName = t.Position?.PositionName,
                ReceivedDate = t.ReceivedDate,
                FirstViewDate = t.FirstViewDate,
                CompletionDate = t.CompletionDate,
                Status = t.Status
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost("refer")]
    public async Task<IActionResult> ReferDocument([FromBody] WorkflowReferRequest dto)
    {
        try
        {
            var currentUserId = _userContextService.GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var document = await _context.Set<Document>().FindAsync(dto.DocumentId);
            if (document == null)
                return NotFound();

            var actionTypeId = dto.ActionTypeId ?? 1;

            foreach (var recipientId in dto.RecipientIds ?? new List<int>())
            {
                var referral = new DocumentReferral
                {
                    DocumentId = dto.DocumentId,
                    FormId = document.FormId,
                    ActionTypeId = actionTypeId,
                    ReferredByUserId = currentUserId.Value,
                    ReferredToUserId = recipientId,
                    ReferredDate = DateTime.UtcNow,
                    DueDate = dto.DueDate,
                    Notes = dto.Notes,
                    Status = "PENDING"
                };

                _context.Set<DocumentReferral>().Add(referral);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, ErrorMessage = ex.Message });
        }
    }

    [HttpGet("referral-modal/{documentId}")]
    public async Task<IActionResult> ReferralModal(int documentId)
    {
        var document = await _context.Set<Document>().FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null)
        {
            return NotFound();
        }

        var viewModel = new ReferralModalDto
        {
            DocumentId = documentId,
            DocumentNumber = document.DocumentNumber
        };

        return PartialView("_ReferralModal", viewModel);
    }
}
