using Automation.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Automation.Module.Workflow.Services;

public class WorkflowEngineSqlService : IWorkflowEngineSqlService
{
    private readonly IConfiguration _configuration;

    public WorkflowEngineSqlService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetConnectionString() =
        _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not found.");

    public async Task<WorkflowMoveResult> MoveToNextStepAsync(int instanceId, string action, int performedByUserId, string? notes = null, string? result = null, int? targetNodeId = null, CancellationToken cancellationToken = default)
    {
        var connStr = GetConnectionString();
        using var conn = new SqlConnection(connStr);
        using var cmd = new SqlCommand("sp_Workflow_MoveToNextStep", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@InstanceId", SqlDbType.Int).Value = instanceId;
        cmd.Parameters.Add("@Action", SqlDbType.NVarChar).Value = action;
        cmd.Parameters.Add("@PerformedByUserId", SqlDbType.Int).Value = performedByUserId;
        cmd.Parameters.Add("@Notes", SqlDbType.NVarChar).Value = (object?)notes ?? DBNull.Value;
        cmd.Parameters.Add("@Result", SqlDbType.NVarChar).Value = (object?)result ?? DBNull.Value;
        cmd.Parameters.Add("@TargetNodeId", SqlDbType.Int).Value = (object?)targetNodeId ?? DBNull.Value;

        var outSuccess = cmd.Parameters.Add("@Success", SqlDbType.Bit); outSuccess.Direction = ParameterDirection.Output;
        var outError = cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, 500); outError.Direction = ParameterDirection.Output;

        try
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return new WorkflowMoveResult
            {
                Success = outSuccess.Value is true,
                ErrorMessage = outError.Value is string s ? s : null
            };
        }
        catch (SqlException ex)
        {
            // Log exception here
            return new WorkflowMoveResult
            {
                Success = false,
                ErrorMessage = $"Database error: {ex.Message}"
            };
        }
    }

    public async Task<WorkflowStartResult> StartInstanceAsync(int workflowId, int startedByUserId, int? workflowVersionId = null, long? formRecordId = null, int? formId = null, CancellationToken cancellationToken = default)
    {
        var connStr = GetConnectionString();
        using var conn = new SqlConnection(connStr);
        using var cmd = new SqlCommand("sp_Workflow_StartInstance", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@WorkflowId", SqlDbType.Int).Value = workflowId;
        cmd.Parameters.Add("@WorkflowVersionId", SqlDbType.Int).Value = (object?)workflowVersionId ?? DBNull.Value;
        cmd.Parameters.Add("@StartedByUserId", SqlDbType.Int).Value = startedByUserId;
        cmd.Parameters.Add("@FormRecordId", SqlDbType.BigInt).Value = (object?)formRecordId ?? DBNull.Value;
        cmd.Parameters.Add("@FormId", SqlDbType.Int).Value = (object?)formId ?? DBNull.Value;

        var outInstanceId = cmd.Parameters.Add("@NewInstanceId", SqlDbType.Int); outInstanceId.Direction = ParameterDirection.Output;
        var outSuccess = cmd.Parameters.Add("@Success", SqlDbType.Bit); outSuccess.Direction = ParameterDirection.Output;
        var outError = cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, 500); outError.Direction = ParameterDirection.Output;

        try
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return new WorkflowStartResult
            {
                Success = outSuccess.Value is true,
                NewInstanceId = outInstanceId.Value is int i ? i : 0,
                ErrorMessage = outError.Value is string s ? s : null
            };
        }
        catch (SqlException ex)
        {
            // Log exception here
            return new WorkflowStartResult
            {
                Success = false,
                NewInstanceId = 0,
                ErrorMessage = $"Database error: {ex.Message}"
            };
        }
    }
}