namespace Automation.Core.Entities;

/// <summary>
/// جریان ترتیب بین نودها (BPMN)
/// </summary>
public class SequenceFlow : BaseEntity
{
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;
    public int SourceNodeId { get; set; }
    public virtual WorkflowNode SourceNode { get; set; } = null!;
    public int TargetNodeId { get; set; }
    public virtual WorkflowNode TargetNode { get; set; } = null!;
    public string? Condition { get; set; }
    public int Order { get; set; }
}
