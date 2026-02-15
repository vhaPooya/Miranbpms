using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Automation.Infrastructure.Services.Workflow;

/// <summary>
/// موتور اجرایی فرآیندهای کسب‌وکار
/// </summary>
public class WorkflowEngine
{
    private readonly AutomationDbContext _context;

    public WorkflowEngine(AutomationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// اجرای یک نمونه فرآیند
    /// </summary>
    public async Task<bool> ExecuteWorkflowInstanceAsync(int workflowInstanceId)
    {
        try
        {
            var workflowInstance = await _context.Set<WorkflowInstance>()
                .Include(wi => wi.Workflow)
                    .ThenInclude(wd => wd.Nodes)
                .Include(wi => wi.CurrentTokens)
                .FirstOrDefaultAsync(wi => wi.Id == workflowInstanceId);

            if (workflowInstance == null)
                return false;

            // پردازش توکن‌های فعال
            foreach (var token in workflowInstance.CurrentTokens.Where(t => t.Status == "ACTIVE"))
            {
                await ProcessTokenAsync(token, workflowInstance);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // ثبت خطا در لاگ
            await LogWorkflowError(workflowInstanceId, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// پردازش یک توکن فرآیند
    /// </summary>
    private async Task ProcessTokenAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        var currentNode = workflowInstance.Workflow.Nodes
            .FirstOrDefault(n => n.Id == token.NodeId);

        if (currentNode == null)
            return;

        var nodeTypeCode = string.IsNullOrEmpty(currentNode.NodeTypeCode) ? currentNode.NodeType.ToString() : currentNode.NodeTypeCode;
        switch (nodeTypeCode)
        {
            case "START_EVENT":
                await ProcessStartEventAsync(token, workflowInstance);
                break;
            case "TASK":
                await ProcessTaskAsync(token, workflowInstance);
                break;
            default:
                await ProcessTaskAsync(token, workflowInstance);
                break;
            case "USER_TASK":
                await ProcessUserTaskAsync(token, workflowInstance);
                break;
            case "SERVICE_TASK":
                await ProcessServiceTaskAsync(token, workflowInstance);
                break;
            case "EXCLUSIVE_GATEWAY":
                await ProcessExclusiveGatewayAsync(token, workflowInstance);
                break;
            case "PARALLEL_GATEWAY":
                await ProcessParallelGatewayAsync(token, workflowInstance);
                break;
            case "END_EVENT":
                await ProcessEndEventAsync(token, workflowInstance);
                break;
        }
    }

    /// <summary>
    /// پردازش نود وظیفه ساده (TASK)
    /// </summary>
    private async Task ProcessTaskAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        var nextNodes = await GetNextNodesAsync(token.NodeId, workflowInstance.WorkflowDefinitionId);
        foreach (var nextNode in nextNodes)
        {
            var newToken = new WorkflowToken
            {
                WorkflowInstanceId = workflowInstance.Id,
                NodeId = nextNode.Id,
                Status = "ACTIVE",
                CreatedDate = DateTime.UtcNow
            };
            _context.Set<WorkflowToken>().Add(newToken);
        }
        token.Status = "COMPLETED";
        token.CompletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// پردازش رویداد شروع
    /// </summary>
    private async Task ProcessStartEventAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        // یافتن گره‌های بعدی
        var nextNodes = await GetNextNodesAsync(token.NodeId, workflowInstance.WorkflowDefinitionId);

        foreach (var nextNode in nextNodes)
        {
            // ایجاد توکن جدید برای گره بعدی
            var newToken = new WorkflowToken
            {
                WorkflowInstanceId = workflowInstance.Id,
                NodeId = nextNode.Id,
                Status = "ACTIVE",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<WorkflowToken>().Add(newToken);
        }

        // تغییر وضعیت توکن فعلی به تکمیل شده
        token.Status = "COMPLETED";
        token.CompletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// پردازش وظیفه کاربر
    /// </summary>
    private async Task ProcessUserTaskAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        // بررسی آیا وظیفه اختصاص داده شده است
        var taskAssignment = await _context.Set<TaskAssignment>()
            .FirstOrDefaultAsync(ta => ta.WorkflowTokenId == token.Id && ta.Status == "ASSIGNED");

        if (taskAssignment == null)
        {
            // اختصاص وظیفه به کاربر یا گروه
            await AssignTaskToUserAsync(token, workflowInstance);
        }
        else if (taskAssignment.Status == "COMPLETED")
        {
            // پردازش تکمیل وظیفه
            await CompleteUserTaskAsync(token, workflowInstance, taskAssignment);
        }
    }

    /// <summary>
    /// اختصاص وظیفه به کاربر
    /// </summary>
    private async Task AssignTaskToUserAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        var node = await _context.Set<WorkflowNode>()
            .FirstOrDefaultAsync(n => n.Id == token.NodeId);

        if (node?.AssigneeType != null)
        {
            var assignment = new TaskAssignment
            {
                WorkflowInstanceId = workflowInstance.Id,
                WorkflowTokenId = token.Id,
                NodeId = token.NodeId,
                AssignmentType = node.AssigneeType ?? "USER",
                AssigneeId = node.AssigneeId ?? 0,
                AssignedDate = DateTime.UtcNow,
                Status = "ASSIGNED",
                DueDate = node.DueDate
            };

            _context.Set<TaskAssignment>().Add(assignment);

            // ایجاد اعلان
            await CreateNotificationAsync(assignment);
        }
    }

    /// <summary>
    /// تکمیل وظیفه کاربر
    /// </summary>
    private async Task CompleteUserTaskAsync(WorkflowToken token, WorkflowInstance workflowInstance, TaskAssignment taskAssignment)
    {
        // یافتن گره‌های بعدی
        var nextNodes = await GetNextNodesAsync(token.NodeId, workflowInstance.WorkflowDefinitionId);

        foreach (var nextNode in nextNodes)
        {
            // ایجاد توکن جدید برای گره بعدی
            var newToken = new WorkflowToken
            {
                WorkflowInstanceId = workflowInstance.Id,
                NodeId = nextNode.Id,
                Status = "ACTIVE",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<WorkflowToken>().Add(newToken);
        }

        // تغییر وضعیت توکن فعلی به تکمیل شده
        token.Status = "COMPLETED";
        token.CompletedDate = DateTime.UtcNow;

        // تغییر وضعیت اختصاص وظیفه
        taskAssignment.Status = "COMPLETED";
        taskAssignment.CompletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// پردازش وظیفه سرویس
    /// </summary>
    private async Task ProcessServiceTaskAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        var node = await _context.Set<WorkflowNode>()
            .FirstOrDefaultAsync(n => n.Id == token.NodeId);

        if (node?.ServiceClass != null)
        {
            try
            {
                // فراخوانی سرویس خارجی
                var result = await ExecuteExternalServiceAsync(node.ServiceClass, node.ServiceMethod, node.ServiceParameters);

                // ذخیره نتیجه در متادیتای توکن
                token.Metadata = JsonSerializer.Serialize(new { ServiceResult = result });
                token.Status = "COMPLETED";
                token.CompletedDate = DateTime.UtcNow;

                // یافتن گره‌های بعدی
                var nextNodes = await GetNextNodesAsync(token.NodeId, workflowInstance.WorkflowDefinitionId);

                foreach (var nextNode in nextNodes)
                {
                    var newToken = new WorkflowToken
                    {
                        WorkflowInstanceId = workflowInstance.Id,
                        NodeId = nextNode.Id,
                        Status = "ACTIVE",
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Set<WorkflowToken>().Add(newToken);
                }
            }
            catch (Exception ex)
            {
                token.Status = "ERROR";
                token.ErrorMessage = ex.Message;
                await LogWorkflowError(workflowInstance.Id, $"Service task error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// پردازش دروازه انحصاری
    /// </summary>
    private async Task ProcessExclusiveGatewayAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        var outgoingFlows = await _context.Set<SequenceFlow>()
            .Where(sf => sf.SourceNodeId == token.NodeId)
            .ToListAsync();

        // یافتن جریان مناسب بر اساس شرایط
        SequenceFlow? selectedFlow = null;
        foreach (var flow in outgoingFlows)
        {
            if (await EvaluateConditionAsync(flow.Condition, workflowInstance))
            {
                selectedFlow = flow;
                break;
            }
        }

        if (selectedFlow != null)
        {
            // ایجاد توکن جدید برای گره مقصد
            var newToken = new WorkflowToken
            {
                WorkflowInstanceId = workflowInstance.Id,
                NodeId = selectedFlow.TargetNodeId,
                Status = "ACTIVE",
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<WorkflowToken>().Add(newToken);
        }

        // تغییر وضعیت توکن فعلی
        token.Status = "COMPLETED";
        token.CompletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// پردازش دروازه موازی
    /// </summary>
    private async Task ProcessParallelGatewayAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        var incomingFlows = await _context.Set<SequenceFlow>()
            .CountAsync(sf => sf.TargetNodeId == token.NodeId);

        var completedIncomingTokens = await _context.Set<WorkflowToken>()
            .CountAsync(t => t.NodeId == token.NodeId && t.Status == "COMPLETED");

        // اگر تمام توکن‌های ورودی تکمیل شده‌اند
        if (completedIncomingTokens >= incomingFlows)
        {
            var outgoingFlows = await _context.Set<SequenceFlow>()
                .Where(sf => sf.SourceNodeId == token.NodeId)
                .ToListAsync();

            foreach (var flow in outgoingFlows)
            {
                // ایجاد توکن برای هر جریان خروجی
                var newToken = new WorkflowToken
                {
                    WorkflowInstanceId = workflowInstance.Id,
                    NodeId = flow.TargetNodeId,
                    Status = "ACTIVE",
                    CreatedDate = DateTime.UtcNow
                };

                _context.Set<WorkflowToken>().Add(newToken);
            }

            token.Status = "COMPLETED";
            token.CompletedDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// پردازش رویداد پایان
    /// </summary>
    private async Task ProcessEndEventAsync(WorkflowToken token, WorkflowInstance workflowInstance)
    {
        token.Status = "COMPLETED";
        token.CompletedDate = DateTime.UtcNow;

        // تغییر وضعیت نمونه فرآیند
        workflowInstance.Status = "COMPLETED";
        workflowInstance.EndedDate = DateTime.UtcNow;

        // ایجاد لاگ پایان فرآیند
        await LogWorkflowActivity(workflowInstance.Id, "PROCESS_ENDED", "فرآیند تکمیل شد");
    }

    /// <summary>
    /// یافتن گره‌های بعدی
    /// </summary>
    private async Task<List<WorkflowNode>> GetNextNodesAsync(int currentNodeId, int workflowDefinitionId)
    {
        var nextNodeIds = await _context.Set<SequenceFlow>()
            .Where(sf => sf.SourceNodeId == currentNodeId)
            .Select(sf => sf.TargetNodeId)
            .ToListAsync();

        return await _context.Set<WorkflowNode>()
            .Where(n => nextNodeIds.Contains(n.Id) && n.WorkflowDefinitionId == workflowDefinitionId)
            .ToListAsync();
    }

    /// <summary>
    /// ارزیابی شرط جریان
    /// </summary>
    private async Task<bool> EvaluateConditionAsync(string condition, WorkflowInstance workflowInstance)
    {
        if (string.IsNullOrEmpty(condition))
            return true;

        // اینجا می‌توان از Expression Tree یا موتور اسکریپت استفاده کرد
        // برای سادگی، شرط ساده را در نظر می‌گیریم
        try
        {
            // مثال: "${document.amount} > 1000"
            // در عمل باید شرط را تجزیه و ارزیابی کرد
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// اجرای سرویس خارجی
    /// </summary>
    private async Task<object> ExecuteExternalServiceAsync(string serviceClass, string serviceMethod, string parameters)
    {
        // اینجا می‌توان از Reflection یا سرویس‌های مجزا استفاده کرد
        return new { Result = "Service executed successfully" };
    }

    /// <summary>
    /// ایجاد اعلان برای کاربر
    /// </summary>
    private async Task CreateNotificationAsync(TaskAssignment assignment)
    {
        var notification = new Notification
        {
            UserId = assignment.AssigneeId,
            Title = "وظیفه جدید",
            Message = "وظیفه جدیدی به شما اختصاص داده شده است",
            Type = "TASK_ASSIGNED",
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ثبت خطا در لاگ فرآیند
    /// </summary>
    private async Task LogWorkflowError(int workflowInstanceId, string errorMessage)
    {
        var logEntry = new WorkflowLog
        {
            WorkflowInstanceId = workflowInstanceId,
            LogLevel = "ERROR",
            Message = errorMessage,
            Timestamp = DateTime.UtcNow
        };

        _context.Set<WorkflowLog>().Add(logEntry);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ثبت فعالیت در لاگ فرآیند
    /// </summary>
    private async Task LogWorkflowActivity(int workflowInstanceId, string activityType, string message)
    {
        var logEntry = new WorkflowLog
        {
            WorkflowInstanceId = workflowInstanceId,
            LogLevel = "INFO",
            ActivityType = activityType,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        _context.Set<WorkflowLog>().Add(logEntry);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ایجاد نمونه جدید فرآیند
    /// </summary>
    public async Task<int> CreateWorkflowInstanceAsync(int workflowDefinitionId, int starterUserId, object inputData)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ایجاد نمونه فرآیند
            var workflowInstance = new WorkflowInstance
            {
                WorkflowDefinitionId = workflowDefinitionId,
                StartedByUserId = starterUserId,
                Status = "RUNNING",
                StartedDate = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow,
                InputData = JsonSerializer.Serialize(inputData),
                State = WorkflowInstanceState.Running,
                WorkflowVersionId = 1
            };

            _context.Set<WorkflowInstance>().Add(workflowInstance);
            await _context.SaveChangesAsync();

            // یافتن گره شروع
            var startNode = await _context.Set<WorkflowNode>()
                .FirstOrDefaultAsync(n => n.WorkflowId == workflowDefinitionId && (n.NodeTypeCode == "START_EVENT" || n.NodeType == WorkflowNodeType.Start));

            if (startNode != null)
            {
                // ایجاد توکن اولیه
                var initialToken = new WorkflowToken
                {
                    WorkflowInstanceId = workflowInstance.Id,
                    NodeId = startNode.Id,
                    Status = "ACTIVE",
                    CreatedDate = DateTime.UtcNow
                };

                _context.Set<WorkflowToken>().Add(initialToken);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // اجرای فرآیند
            await ExecuteWorkflowInstanceAsync(workflowInstance.Id);

            return workflowInstance.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}