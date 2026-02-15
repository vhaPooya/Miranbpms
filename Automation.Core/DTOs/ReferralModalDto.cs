namespace Automation.Core.DTOs;

public class ReferralModalDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public List<ReferralRecipientDto> Recipients { get; set; } = new List<ReferralRecipientDto>();
    public List<ActionTypeDto> ActionTypes { get; set; } = new List<ActionTypeDto>();
}

public class ReferralRecipientDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}

public class ActionTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}