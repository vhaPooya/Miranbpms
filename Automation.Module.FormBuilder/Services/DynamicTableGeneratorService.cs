using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Automation.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Infrastructure.Data;

namespace Automation.Module.FormBuilder.Services;

/// <summary>
/// تبدیل طراحی JSON فرم به اسکریپت CREATE TABLE فیزیکی و اجرای آن (دامنه داینامیک با SQL خام).
/// </summary>
public interface IDynamicTableGeneratorService
{
    /// <summary>
    /// تولید و اجرای CREATE TABLE بر اساس فرم و فیلدها (یا DesignData JSON).
    /// </summary>
    Task<bool> EnsureTableFromFormDesignAsync(Form form, List<FormField> fields, CancellationToken cancellationToken = default);
    /// <summary>
    /// تولید اسکریپت CREATE TABLE بدون اجرا (برای پیش‌نمایش یا ذخیره).
    /// </summary>
    string GenerateCreateTableScript(string tableName, List<FormField> fields);
}

public class DynamicTableGeneratorService : IDynamicTableGeneratorService
{
    private readonly AutomationDbContext _context;

    public DynamicTableGeneratorService(AutomationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EnsureTableFromFormDesignAsync(Form form, List<FormField> fields, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(form.DatabaseTableName))
            return false;

        var tableName = FilterTableName(form.DatabaseTableName);
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var exists = await TableExistsAsync(connection, tableName, cancellationToken).ConfigureAwait(false);
        if (exists)
            return await UpdateTableAsync(connection, tableName, fields, cancellationToken).ConfigureAwait(false);

        var script = GenerateCreateTableScript(tableName, fields);
        await ExecuteScriptAsync(connection, script, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public string GenerateCreateTableScript(string tableName, List<FormField> fields)
    {
        var safeName = FilterTableName(tableName);
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE [{safeName}] (");
        sb.AppendLine("    [Id] INT IDENTITY(1,1) PRIMARY KEY,");
        sb.AppendLine("    [CreatedAt] DATETIME2 DEFAULT SYSUTCDATETIME(),");
        sb.AppendLine("    [UpdatedAt] DATETIME2 NULL,");
        sb.AppendLine("    [CreatedBy] INT NULL");

        foreach (var f in fields.Where(x => !string.IsNullOrEmpty(x.DatabaseColumnName)))
        {
            var col = FilterColumnName(f.DatabaseColumnName!);
            var sqlType = MapToSqlType(f);
            var nullable = f.IsNullable ? "NULL" : "NOT NULL";
            sb.AppendLine($"    , [{col}] {sqlType} {nullable}");
        }

        sb.AppendLine(");");
        return sb.ToString();
    }

    private static string FilterTableName(string name)
    {
        return Regex.Replace(name, @"[^a-zA-Z0-9_]", "");
    }

    private static string FilterColumnName(string name)
    {
        return Regex.Replace(name, @"[^a-zA-Z0-9_]", "");
    }

    private static string MapToSqlType(FormField f)
    {
        var type = (f.DatabaseColumnType ?? "").ToLowerInvariant();
        if (type.Contains("int")) return "INT";
        if (type.Contains("bigint")) return "BIGINT";
        if (type.Contains("decimal") || type.Contains("numeric")) return "DECIMAL(18,4)";
        if (type.Contains("date") && !type.Contains("time")) return "DATE";
        if (type.Contains("datetime")) return "DATETIME2";
        if (type.Contains("bit") || type.Contains("bool")) return "BIT";
        if (type.Contains("float") || type.Contains("double")) return "FLOAT";
        return "NVARCHAR(MAX)";
    }

    private async Task<bool> TableExistsAsync(System.Data.Common.DbConnection connection, string tableName, CancellationToken cancellationToken)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @name";
        var p = cmd.CreateParameter();
        p.ParameterName = "name";
        p.Value = tableName;
        cmd.Parameters.Add(p);
        var r = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return r != null && Convert.ToInt32(r) > 0;
    }

    private async Task<bool> UpdateTableAsync(System.Data.Common.DbConnection connection, string tableName, List<FormField> fields, CancellationToken cancellationToken)
    {
        foreach (var f in fields.Where(x => !string.IsNullOrEmpty(x.DatabaseColumnName)))
        {
            var col = FilterColumnName(f.DatabaseColumnName!);
            var exists = await ColumnExistsAsync(connection, tableName, col, cancellationToken).ConfigureAwait(false);
            if (exists) continue;
            var sqlType = MapToSqlType(f);
            var script = $"ALTER TABLE [{tableName}] ADD [{col}] {sqlType} {(f.IsNullable ? "NULL" : "NOT NULL")};";
            await ExecuteScriptAsync(connection, script, cancellationToken).ConfigureAwait(false);
        }
        return true;
    }

    private async Task<bool> ColumnExistsAsync(System.Data.Common.DbConnection connection, string tableName, string columnName, CancellationToken cancellationToken)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t AND COLUMN_NAME = @c";
        var pt = cmd.CreateParameter(); pt.ParameterName = "t"; pt.Value = tableName; cmd.Parameters.Add(pt);
        var pc = cmd.CreateParameter(); pc.ParameterName = "c"; pc.Value = columnName; cmd.Parameters.Add(pc);
        var r = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return r != null && Convert.ToInt32(r) > 0;
    }

    private async Task ExecuteScriptAsync(System.Data.Common.DbConnection connection, string script, CancellationToken cancellationToken)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = script;
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
