namespace Automation.Core.DTOs;

public class WorkflowVisualizationDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public List<TrackingNodeDto> Trackings { get; set; } = new List<TrackingNodeDto>();
}

public class TrackingNodeDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? PositionId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public DateTime? FirstViewDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}