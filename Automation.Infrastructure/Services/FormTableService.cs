using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

public class FormTableService : IFormTableService
{
    private readonly AutomationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FormTableService> _logger;

    public FormTableService(
        AutomationDbContext context,
        IConfiguration configuration,
        ILogger<FormTableService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> CreateFormTableAsync(Form form, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Connection string not found");

            var tableName = form.DatabaseTableName ?? $"Form_{form.FormCode.Replace("-", "_")}";
            
            // Get form fields
            var fields = await _context.FormFields
                .Include(f => f.FieldType)
                .Where(f => f.FormId == form.Id && !f.IsDeleted)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync(cancellationToken);

            if (!fields.Any())
                throw new Exception("Form has no fields");

            // Build CREATE TABLE SQL
            var sql = BuildCreateTableSql(tableName, fields);
            
            // Execute SQL
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            
            // Save table info
            var formDataTable = await _context.FormDataTables
                .FirstOrDefaultAsync(t => t.FormId == form.Id, cancellationToken);
                
            if (formDataTable == null)
            {
                formDataTable = new FormDataTable
                {
                    FormId = form.Id,
                    TableName = tableName,
                    IsTableCreated = true,
                    TableCreatedDate = DateTime.UtcNow,
                    TableSchema = sql
                };
                _context.FormDataTables.Add(formDataTable);
            }
            else
            {
                formDataTable.TableName = tableName;
                formDataTable.IsTableCreated = true;
                formDataTable.TableCreatedDate = DateTime.UtcNow;
                formDataTable.TableSchema = sql;
            }
            
            form.DatabaseTableName = tableName;
            form.IsTableCreated = true;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            // Create Stored Procedures
            await CreateStoredProceduresAsync(tableName, fields, connection, cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form table for form {FormId}", form.Id);
            return false;
        }
    }

    private string BuildCreateTableSql(string tableName, List<FormField> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{tableName}]') AND type in (N'U'))");
        sb.AppendLine("BEGIN");
        sb.AppendLine($"    CREATE TABLE [dbo].[{tableName}] (");
        sb.AppendLine("        [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),");
        sb.AppendLine("        [CreationDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),");
        sb.AppendLine("        [CreatorUserId] UNIQUEIDENTIFIER NULL,");
        sb.AppendLine("        [CreatorRoleId] UNIQUEIDENTIFIER NULL,");
        sb.AppendLine("        [EditDate] DATETIME2 NULL,");
        sb.AppendLine("        [IsActive] BIT NOT NULL DEFAULT 1,");
        sb.AppendLine("        [IsDeleted] BIT NOT NULL DEFAULT 0,");
        
        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field.DatabaseColumnName))
                continue;
                
            var columnName = field.DatabaseColumnName;
            var dataType = GetSqlDataType(field);
            var nullable = field.IsNullable ? "NULL" : "NOT NULL";
            
            sb.AppendLine($"        [{columnName}] {dataType} {nullable},");
        }
        
        sb.AppendLine("        [CreatedAt] AS [CreationDate],");
        sb.AppendLine("        [UpdatedAt] AS [EditDate]");
        sb.AppendLine("    );");
        sb.AppendLine("END");
        
        return sb.ToString();
    }

    private string GetSqlDataType(FormField field)
    {
        var dataType = field.DatabaseColumnType?.ToLower() ?? "nvarchar";
        
        switch (dataType)
        {
            case "int":
            case "bigint":
            case "smallint":
            case "tinyint":
                return dataType;
            case "decimal":
            case "numeric":
                return $"{dataType}(18,2)";
            case "float":
                return "FLOAT";
            case "bit":
                return "BIT";
            case "date":
                return "DATE";
            case "datetime":
            case "datetime2":
                return "DATETIME2";
            case "time":
                return "TIME";
            case "uniqueidentifier":
                return "UNIQUEIDENTIFIER";
            case "nvarchar":
            case "varchar":
            default:
                var maxLength = field.MaxLength > 0 ? field.MaxLength : 500;
                if (maxLength == -1 || maxLength > 4000)
                    return "NVARCHAR(MAX)";
                return $"NVARCHAR({maxLength})";
        }
    }

    private async Task CreateStoredProceduresAsync(string tableName, List<FormField> fields, SqlConnection connection, CancellationToken cancellationToken)
    {
        // Create SP_Insert
        var insertSp = BuildInsertStoredProcedure(tableName, fields);
        await ExecuteSqlAsync(insertSp, connection, cancellationToken);
        
        // Create SP_Update
        var updateSp = BuildUpdateStoredProcedure(tableName, fields);
        await ExecuteSqlAsync(updateSp, connection, cancellationToken);
        
        // Create SP_Delete
        var deleteSp = BuildDeleteStoredProcedure(tableName);
        await ExecuteSqlAsync(deleteSp, connection, cancellationToken);
        
        // Create SP_GetById
        var getByIdSp = BuildGetByIdStoredProcedure(tableName);
        await ExecuteSqlAsync(getByIdSp, connection, cancellationToken);
        
        // Create SP_GetAll
        var getAllSp = BuildGetAllStoredProcedure(tableName);
        await ExecuteSqlAsync(getAllSp, connection, cancellationToken);
    }

    private string BuildInsertStoredProcedure(string tableName, List<FormField> fields)
    {
        var sb = new StringBuilder();
        var spName = $"SP_{tableName}_Insert";
        
        sb.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{spName}]') AND type in (N'P', N'PC'))");
        sb.AppendLine($"    DROP PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("GO");
        sb.AppendLine();
        sb.AppendLine($"CREATE PROCEDURE [dbo].[{spName}]");
        
        var parameters = new List<string>();
        foreach (var field in fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName)))
        {
            var paramName = $"@{field.DatabaseColumnName}";
            var sqlType = GetSqlDataType(field);
            parameters.Add($"{paramName} {sqlType}");
        }
        
        parameters.Add("@CreatorUserId UNIQUEIDENTIFIER = NULL");
        parameters.Add("@CreatorRoleId UNIQUEIDENTIFIER = NULL");
        
        sb.AppendLine(string.Join(",\n", parameters));
        sb.AppendLine("AS");
        sb.AppendLine("BEGIN");
        sb.AppendLine("    SET NOCOUNT ON;");
        sb.AppendLine();
        sb.AppendLine($"    INSERT INTO [dbo].[{tableName}] (");
        
        var columns = new List<string> { "Id", "CreationDate", "CreatorUserId", "CreatorRoleId", "IsActive", "IsDeleted" };
        columns.AddRange(fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName))
            .Select(f => f.DatabaseColumnName!));
        
        sb.AppendLine("        " + string.Join(",\n        ", columns.Select(c => $"[{c}]")));
        sb.AppendLine("    ) VALUES (");
        
        var values = new List<string> { "NEWID()", "GETUTCDATE()", "@CreatorUserId", "@CreatorRoleId", "1", "0" };
        values.AddRange(fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName))
            .Select(f => $"@{f.DatabaseColumnName}"));
        
        sb.AppendLine("        " + string.Join(",\n        ", values));
        sb.AppendLine("    );");
        sb.AppendLine();
        sb.AppendLine("    SELECT SCOPE_IDENTITY() AS Id;");
        sb.AppendLine("END");
        sb.AppendLine("GO");
        
        return sb.ToString();
    }

    private string BuildUpdateStoredProcedure(string tableName, List<FormField> fields)
    {
        var sb = new StringBuilder();
        var spName = $"SP_{tableName}_Update";
        
        sb.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{spName}]') AND type in (N'P', N'PC'))");
        sb.AppendLine($"    DROP PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("GO");
        sb.AppendLine();
        sb.AppendLine($"CREATE PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("    @Id UNIQUEIDENTIFIER,");
        
        var parameters = new List<string>();
        foreach (var field in fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName)))
        {
            var paramName = $"@{field.DatabaseColumnName}";
            var sqlType = GetSqlDataType(field);
            parameters.Add($"{paramName} {sqlType}");
        }
        
        sb.AppendLine(string.Join(",\n    ", parameters));
        sb.AppendLine("AS");
        sb.AppendLine("BEGIN");
        sb.AppendLine("    SET NOCOUNT ON;");
        sb.AppendLine();
        sb.AppendLine($"    UPDATE [dbo].[{tableName}]");
        sb.AppendLine("    SET");
        
        var sets = new List<string> { "EditDate = GETUTCDATE()" };
        sets.AddRange(fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName))
            .Select(f => $"[{f.DatabaseColumnName}] = @{f.DatabaseColumnName}"));
        
        sb.AppendLine("        " + string.Join(",\n        ", sets));
        sb.AppendLine("    WHERE Id = @Id AND IsDeleted = 0;");
        sb.AppendLine();
        sb.AppendLine("    SELECT @@ROWCOUNT AS RowsAffected;");
        sb.AppendLine("END");
        sb.AppendLine("GO");
        
        return sb.ToString();
    }

    private string BuildDeleteStoredProcedure(string tableName)
    {
        var sb = new StringBuilder();
        var spName = $"SP_{tableName}_Delete";
        
        sb.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{spName}]') AND type in (N'P', N'PC'))");
        sb.AppendLine($"    DROP PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("GO");
        sb.AppendLine();
        sb.AppendLine($"CREATE PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("    @Id UNIQUEIDENTIFIER");
        sb.AppendLine("AS");
        sb.AppendLine("BEGIN");
        sb.AppendLine("    SET NOCOUNT ON;");
        sb.AppendLine();
        sb.AppendLine($"    UPDATE [dbo].[{tableName}]");
        sb.AppendLine("    SET IsDeleted = 1, EditDate = GETUTCDATE()");
        sb.AppendLine("    WHERE Id = @Id;");
        sb.AppendLine();
        sb.AppendLine("    SELECT @@ROWCOUNT AS RowsAffected;");
        sb.AppendLine("END");
        sb.AppendLine("GO");
        
        return sb.ToString();
    }

    private string BuildGetByIdStoredProcedure(string tableName)
    {
        var sb = new StringBuilder();
        var spName = $"SP_{tableName}_GetById";
        
        sb.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{spName}]') AND type in (N'P', N'PC'))");
        sb.AppendLine($"    DROP PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("GO");
        sb.AppendLine();
        sb.AppendLine($"CREATE PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("    @Id UNIQUEIDENTIFIER");
        sb.AppendLine("AS");
        sb.AppendLine("BEGIN");
        sb.AppendLine("    SET NOCOUNT ON;");
        sb.AppendLine();
        sb.AppendLine($"    SELECT * FROM [dbo].[{tableName}]");
        sb.AppendLine("    WHERE Id = @Id AND IsDeleted = 0;");
        sb.AppendLine("END");
        sb.AppendLine("GO");
        
        return sb.ToString();
    }

    private string BuildGetAllStoredProcedure(string tableName)
    {
        var sb = new StringBuilder();
        var spName = $"SP_{tableName}_GetAll";
        
        sb.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{spName}]') AND type in (N'P', N'PC'))");
        sb.AppendLine($"    DROP PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("GO");
        sb.AppendLine();
        sb.AppendLine($"CREATE PROCEDURE [dbo].[{spName}]");
        sb.AppendLine("    @PageNumber INT = 1,");
        sb.AppendLine("    @PageSize INT = 10,");
        sb.AppendLine("    @SearchTerm NVARCHAR(500) = NULL");
        sb.AppendLine("AS");
        sb.AppendLine("BEGIN");
        sb.AppendLine("    SET NOCOUNT ON;");
        sb.AppendLine();
        sb.AppendLine($"    SELECT * FROM [dbo].[{tableName}]");
        sb.AppendLine("    WHERE IsDeleted = 0");
        sb.AppendLine("        AND (@SearchTerm IS NULL OR ...) -- Add search logic");
        sb.AppendLine($"    ORDER BY CreationDate DESC");
        sb.AppendLine("    OFFSET (@PageNumber - 1) * @PageSize ROWS");
        sb.AppendLine("    FETCH NEXT @PageSize ROWS ONLY;");
        sb.AppendLine();
        sb.AppendLine($"    SELECT COUNT(*) AS TotalCount FROM [dbo].[{tableName}] WHERE IsDeleted = 0;");
        sb.AppendLine("END");
        sb.AppendLine("GO");
        
        return sb.ToString();
    }

    private async Task ExecuteSqlAsync(string sql, SqlConnection connection, CancellationToken cancellationToken)
    {
        using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<bool> UpdateFormTableAsync(Form form, CancellationToken cancellationToken = default)
    {
        // TODO: Implement table update logic
        return Task.FromResult(false);
    }

    public Task<bool> DropFormTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
        // TODO: Implement table drop logic
        return Task.FromResult(false);
    }

    public Task<string> GetTableSchemaAsync(string tableName, CancellationToken cancellationToken = default)
    {
        // TODO: Implement get schema logic
        return Task.FromResult(string.Empty);
    }
}




