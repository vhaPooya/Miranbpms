using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace Automation.Infrastructure.Services;

public interface IFormSchemaService
{
    Task EnsureTableStructureAsync(Form form, List<FormField> fields);
}

public class FormSchemaService : IFormSchemaService
{
    private readonly AutomationDbContext _context;

    public FormSchemaService(AutomationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureTableStructureAsync(Form form, List<FormField> fields)
    {
        if (string.IsNullOrEmpty(form.DatabaseTableName)) return;

        var tableName = FilterTableName(form.DatabaseTableName);
        
        // Use EF Context Database facade to access connection
        var connection = _context.Database.GetDbConnection();
        // Ensure connection is open
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();
        
        bool tableExists = false;
        string checkTableSql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
        
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = checkTableSql;
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && int.TryParse(result.ToString(), out int count))
            {
                tableExists = count > 0;
            }
        }

        if (!tableExists)
        {
            await CreateTableAsync(tableName, fields, connection);
        }
        else
        {
            await UpdateTableAsync(tableName, fields, connection);
        }
        
        // We do not close connection here if it's managed by EF, but generally safe to leave open in Scoped request.
    }

    private async Task CreateTableAsync(string tableName, List<FormField> fields, System.Data.Common.DbConnection connection)
    {
        var sb = new StringBuilder();
        var indexFields = new List<string>();
        var uniqueFields = new List<string>();
        
        sb.AppendLine($"CREATE TABLE [{tableName}] (");
        sb.AppendLine("    [Id] INT IDENTITY(1,1) PRIMARY KEY,");
        sb.AppendLine("    [CreatedAt] DATETIME2 DEFAULT SYSUTCDATETIME(),");
        sb.AppendLine("    [UpdatedAt] DATETIME2 NULL,");
        sb.AppendLine("    [CreatedBy] NVARCHAR(100) NULL,"); 

        foreach (var field in fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName)))
        {
            var colName = FilterColumnName(field.DatabaseColumnName!);
            var type = GetSqlType(field);
            var nullable = field.IsNullable ? "NULL" : "NOT NULL";
            var defaultValue = GetDefaultValue(field);
            
            sb.AppendLine($"    [{colName}] {type} {nullable}{defaultValue},");
            
            // Track fields for index/unique
            if (field.HasIndex)
            {
                indexFields.Add(colName);
            }
            
            // Check if unique - check Properties JSON for isUnique flag
            bool isUnique = false;
            if (field.HasIndex && field.Properties != null)
            {
                try
                {
                    var props = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(field.Properties);
                    if (props != null)
                    {
                        // Check direct isUnique property
                        if (props.ContainsKey("isUnique"))
                        {
                            if (bool.TryParse(props["isUnique"].ToString(), out bool propUnique))
                                isUnique = propUnique;
                        }
                        // Also check in data object
                        else if (props.ContainsKey("data"))
                        {
                            var dataObj = props["data"];
                            if (dataObj != null)
                            {
                                var dataStr = dataObj.ToString();
                                if (!string.IsNullOrEmpty(dataStr))
                                {
                                    var dataDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(dataStr);
                                    if (dataDict != null && dataDict.ContainsKey("isUnique"))
                                    {
                                        if (bool.TryParse(dataDict["isUnique"].ToString(), out bool dataUnique))
                                            isUnique = dataUnique;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            
            if (isUnique)
            {
                uniqueFields.Add(colName);
            }
        }

        sb.Remove(sb.Length - 3, 3); // Remove last comma and newline
        sb.AppendLine("");
        sb.AppendLine(");");

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = sb.ToString();
            await cmd.ExecuteNonQueryAsync();
        }
        
        // Create indexes
        foreach (var colName in indexFields)
        {
            var indexName = $"IX_{tableName}_{colName}";
            var isUnique = uniqueFields.Contains(colName);
            var uniqueClause = isUnique ? "UNIQUE" : "";
            
            var indexSql = $"CREATE {uniqueClause} NONCLUSTERED INDEX [{indexName}] ON [{tableName}] ([{colName}])";
            
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = indexSql;
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    // Log but continue - index might already exist
                    Console.WriteLine($"Warning: Could not create index {indexName}: {ex.Message}");
                }
            }
        }
    }
    
    private string GetDefaultValue(FormField field)
    {
        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            // If it's a constant value, wrap in quotes if it's a string type
            var type = field.DatabaseColumnType?.ToLower() ?? "nvarchar";
            if (type.StartsWith("nvarchar") || type.StartsWith("varchar") || type.StartsWith("char"))
            {
                return $" DEFAULT N'{field.DefaultValue.Replace("'", "''")}'";
            }
            else if (type == "bit" || type == "boolean")
            {
                if (bool.TryParse(field.DefaultValue, out bool boolVal))
                {
                    return $" DEFAULT {(boolVal ? "1" : "0")}";
                }
            }
            else if (type == "int" || type == "bigint" || type == "smallint")
            {
                if (int.TryParse(field.DefaultValue, out _))
                {
                    return $" DEFAULT {field.DefaultValue}";
                }
            }
            else
            {
                return $" DEFAULT {field.DefaultValue}";
            }
        }
        
        // Check for default function
        if (field.DefaultFunctionId.HasValue && field.DefaultFunction != null)
        {
            var funcName = field.DefaultFunction.Name;
            return $" DEFAULT {funcName}()";
        }
        
        return "";
    }

    private async Task UpdateTableAsync(string tableName, List<FormField> fields, System.Data.Common.DbConnection connection)
    {
        var existingCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingCols.Add(reader.GetString(0));
                }
            }
        }

        foreach (var field in fields.Where(f => !string.IsNullOrEmpty(f.DatabaseColumnName)))
        {
            var colName = FilterColumnName(field.DatabaseColumnName!);
            if (!existingCols.Contains(colName))
            {
                var type = GetSqlType(field);
                var sql = $"ALTER TABLE [{tableName}] ADD [{colName}] {type} NULL";
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            // Altering existing columns is skipped to prevent data loss risks in this version
        }
    }

    private string GetSqlType(FormField field)
    {
        var type = field.DatabaseColumnType?.ToLower() ?? "nvarchar";
        
        // Remove parentheses and length if present to get base type
        var baseType = type.Contains("(") ? type.Substring(0, type.IndexOf("(")).Trim() : type;
        
        // Handle common types with robust defaults
        if (baseType == "nvarchar" || baseType == "varchar" || baseType == "char" || baseType == "nchar")
        {
             // If length is already in string (e.g. nvarchar(50)), return it as uppercase
             if(type.Contains("(")) 
             {
                 var upperType = type.ToUpper();
                 // Ensure NVARCHAR instead of varchar
                 if (upperType.StartsWith("VARCHAR") || upperType.StartsWith("CHAR"))
                     upperType = "N" + upperType;
                 return upperType;
             }
             
             // Check MaxLength property
             if (field.MaxLength.HasValue)
             {
                 if (field.MaxLength.Value <= 0)
                     return "NVARCHAR(MAX)";
                 return $"NVARCHAR({field.MaxLength.Value})";
             }
                
             return "NVARCHAR(MAX)";
        }
        
        if (baseType == "int" || baseType == "integer") return "INT";
        if (baseType == "bigint") return "BIGINT";
        if (baseType == "smallint") return "SMALLINT";
        if (baseType == "tinyint") return "TINYINT";
        if (baseType == "bit" || baseType == "boolean") return "BIT";
        if (baseType == "date") return "DATE";
        if (baseType == "datetime" || baseType == "datetime2") return "DATETIME2";
        if (baseType == "time") return "TIME(0)";
        if (baseType == "decimal" || baseType == "numeric")
        {
            // Check for precision and scale in Properties
            int precision = 18;
            int scale = 2;
            if (field.Properties != null)
            {
                try
                {
                    var props = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(field.Properties);
                    if (props != null)
                    {
                        if (props.ContainsKey("precision") && int.TryParse(props["precision"].ToString(), out int p))
                            precision = p;
                        if (props.ContainsKey("scale") && int.TryParse(props["scale"].ToString(), out int s))
                            scale = s;
                    }
                }
                catch { }
            }
            return $"DECIMAL({precision}, {scale})";
        }
        if (baseType == "money") return "MONEY";
        if (baseType == "float") return "FLOAT";
        if (baseType == "real") return "REAL";
        if (baseType == "uniqueidentifier" || baseType == "int") return "UNIQUEIDENTIFIER";
        
        // Fallback - return as uppercase
        return type.ToUpper();
    }
    
    private string FilterTableName(string input)
    {
        return Regex.Replace(input, @"[^a-zA-Z0-9_]", "");
    }
    
    private string FilterColumnName(string input)
    {
         return Regex.Replace(input, @"[^a-zA-Z0-9_]", "");
    }
}



