namespace Automation.Core.DTOs;

public class WorkflowDiagramDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public List<DiagramNodeDto> Nodes { get; set; } = new List<DiagramNodeDto>();
    public List<NodeConnectionDto> Connections { get; set; } = new List<NodeConnectionDto>();
}

public class DiagramNodeDto
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
}

public class NodeConnectionDto
{
    public int FromNodeId { get; set; }
    public int ToNodeId { get; set; }
    public string ConnectionType { get; set; } = string.Empty;
    public DateTime ConnectionDate { get; set; }
}