using Automation.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Automation.Module.Workflow.Services;

/// <summary>
/// اجرای موتور گردش کار از طریق Stored Procedureهای SQL (بدون بارگذاری کل گراف در حافظه).
/// </summary>
public interface IWorkflowEngineSqlService
{
    Task<WorkflowMoveResult> MoveToNextStepAsync(int instanceId, string action, int performedByUserId, string? notes = null, string? result = null, int? targetNodeId = null, CancellationToken cancellationToken = default);
    Task<WorkflowStartResult> StartInstanceAsync(int workflowId, int startedByUserId, int? workflowVersionId = null, long? formRecordId = null, int? formId = null, CancellationToken cancellationToken = default);
}

public class WorkflowMoveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WorkflowStartResult
{
    public bool Success { get; set; }
    public int NewInstanceId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WorkflowEngineSqlService : IWorkflowEngineSqlService
{
    private readonly IConfiguration _configuration;

    public WorkflowEngineSqlService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetConnectionString() =>
        _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not found.");

    public async Task<WorkflowMoveResult> MoveToNextStepAsync(int instanceId, string action, int performedByUserId, string? notes = null, string? result = null, int? targetNodeId = null, CancellationToken cancellationToken = default)
    {
        var connStr = GetConnectionString();
        using var conn = new SqlConnection(connStr);
        using var cmd = new SqlCommand("sp_Workflow_MoveToNextStep", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@InstanceId", instanceId);
        cmd.Parameters.AddWithValue("@Action", action);
        cmd.Parameters.AddWithValue("@PerformedByUserId", performedByUserId);
        cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Result", (object?)result ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TargetNodeId", (object?)targetNodeId ?? DBNull.Value);
        var outSuccess = cmd.Parameters.Add("@Success", SqlDbType.Bit); outSuccess.Direction = ParameterDirection.Output;
        var outError = cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, 500); outError.Direction = ParameterDirection.Output;

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return new WorkflowMoveResult
        {
            Success = outSuccess.Value is true,
            ErrorMessage = outError.Value is string s ? s : null
        };
    }

    public async Task<WorkflowStartResult> StartInstanceAsync(int workflowId, int startedByUserId, int? workflowVersionId = null, long? formRecordId = null, int? formId = null, CancellationToken cancellationToken = default)
    {
        var connStr = GetConnectionString();
        using var conn = new SqlConnection(connStr);
        using var cmd = new SqlCommand("sp_Workflow_StartInstance", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@WorkflowId", workflowId);
        cmd.Parameters.AddWithValue("@WorkflowVersionId", (object?)workflowVersionId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StartedByUserId", startedByUserId);
        cmd.Parameters.AddWithValue("@FormRecordId", (object?)formRecordId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FormId", (object?)formId ?? DBNull.Value);
        var outInstanceId = cmd.Parameters.Add("@NewInstanceId", SqlDbType.Int); outInstanceId.Direction = ParameterDirection.Output;
        var outSuccess = cmd.Parameters.Add("@Success", SqlDbType.Bit); outSuccess.Direction = ParameterDirection.Output;
        var outError = cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, 500); outError.Direction = ParameterDirection.Output;

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return new WorkflowStartResult
        {
            Success = outSuccess.Value is true,
            NewInstanceId = outInstanceId.Value is int i ? i : 0,
            ErrorMessage = outError.Value is string s ? s : null
        };
    }
}
