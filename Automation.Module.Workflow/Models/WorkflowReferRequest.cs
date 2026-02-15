namespace Automation.Module.Workflow.Models;

/// <summary>
/// درخواست ارجاع سند از صفحه فرآیند (Workflow)
/// </summary>
public class WorkflowReferRequest
{
    public int DocumentId { get; set; }
    public List<int> RecipientIds { get; set; } = new();
    public string? ActionType { get; set; }
    public int? ActionTypeId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}
