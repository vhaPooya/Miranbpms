using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Automation.Infrastructure.Services.Integration.Database;

/// <summary>
/// سرویس اتصال به پایگاه‌های داده خارجی
/// </summary>
public class DbConnectorService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbConnectorService> _logger;

    public DbConnectorService(IConfiguration configuration, ILogger<DbConnectorService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// دریافت اتصال به پایگاه داده
    /// </summary>
    public IDbConnection GetConnection(string connectionStringName)
    {
        var connectionString = _configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
        }

        return new Microsoft.Data.SqlClient.SqlConnection(connectionString);
    }

    /// <summary>
    /// اجرای کوئری SELECT
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(string connectionStringName, string sql, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.QueryAsync<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute query on {connectionStringName}: {sql}");
            throw new DbConnectorException($"Failed to execute query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// اجرای کوئری با پارامترهای چندگانه
    /// </summary>
    public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string connectionStringName, string sql, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.QueryAsync<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute multiple query on {connectionStringName}: {sql}");
            throw new DbConnectorException($"Failed to execute multiple query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// اجرای دستور INSERT/UPDATE/DELETE
    /// </summary>
    public async Task<int> ExecuteAsync(string connectionStringName, string sql, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.ExecuteAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute command on {connectionStringName}: {sql}");
            throw new DbConnectorException($"Failed to execute command: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// اجرای تراکنش
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(string connectionStringName, Func<IDbConnection, IDbTransaction, Task<T>> operation)
    {
        using var connection = GetConnection(connectionStringName);
        using var transaction = connection.BeginTransaction();
        
        try
        {
            var result = await operation(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, $"Transaction failed on {connectionStringName}");
            throw new DbConnectorException($"Transaction failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// دریافت رکورد واحد
    /// </summary>
    public async Task<T> QuerySingleAsync<T>(string connectionStringName, string sql, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.QuerySingleAsync<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute single query on {connectionStringName}: {sql}");
            throw new DbConnectorException($"Failed to execute single query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// دریافت رکورد واحد یا پیش‌فرض
    /// </summary>
    public async Task<T> QuerySingleOrDefaultAsync<T>(string connectionStringName, string sql, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute single or default query on {connectionStringName}: {sql}");
            throw new DbConnectorException($"Failed to execute single or default query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// اجرای stored procedure
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string connectionStringName, string procedureName, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute stored procedure {procedureName} on {connectionStringName}");
            throw new DbConnectorException($"Failed to execute stored procedure: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// اجرای stored procedure بدون بازگشت داده
    /// </summary>
    public async Task<int> ExecuteStoredProcedureNonQueryAsync(string connectionStringName, string procedureName, object parameters = null)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute stored procedure {procedureName} on {connectionStringName}");
            throw new DbConnectorException($"Failed to execute stored procedure: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// دریافت اطلاعات ساختار جدول
    /// </summary>
    public async Task<TableSchema> GetTableSchemaAsync(string connectionStringName, string tableName)
    {
        try
        {
            var sql = @"
                SELECT 
                    COLUMN_NAME as ColumnName,
                    DATA_TYPE as DataType,
                    IS_NULLABLE as IsNullable,
                    COLUMN_DEFAULT as ColumnDefault,
                    CHARACTER_MAXIMUM_LENGTH as MaxLength,
                    NUMERIC_PRECISION as NumericPrecision,
                    NUMERIC_SCALE as NumericScale
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @TableName
                ORDER BY ORDINAL_POSITION";

            using var connection = GetConnection(connectionStringName);
            var columns = await connection.QueryAsync<ColumnSchema>(sql, new { TableName = tableName });

            return new TableSchema
            {
                TableName = tableName,
                Columns = columns.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get table schema for {tableName} on {connectionStringName}");
            throw new DbConnectorException($"Failed to get table schema: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// تست اتصال
    /// </summary>
    public async Task<bool> TestConnectionAsync(string connectionStringName)
    {
        try
        {
            using var connection = GetConnection(connectionStringName);
            if (connection is Microsoft.Data.SqlClient.SqlConnection sqlConn)
                await sqlConn.OpenAsync();
            else
                connection.Open();
            return connection.State == ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// دریافت لیست جداول
    /// </summary>
    public async Task<List<string>> GetTablesAsync(string connectionStringName)
    {
        try
        {
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            
            using var connection = GetConnection(connectionStringName);
            var tables = await connection.QueryAsync<string>(sql);
            return tables.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get tables list on {connectionStringName}");
            throw new DbConnectorException($"Failed to get tables list: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// دریافت آمار پایگاه داده
    /// </summary>
    public async Task<DatabaseStats> GetDatabaseStatsAsync(string connectionStringName)
    {
        try
        {
            var stats = new DatabaseStats();
            
            // تعداد جداول
            var tableCountSql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using var connection = GetConnection(connectionStringName);
            stats.TableCount = await connection.QuerySingleAsync<int>(tableCountSql);
            
            // تعداد رکوردها در جداول اصلی
            var rowCountSql = @"
                SELECT SUM(row_count) as TotalRows
                FROM sys.dm_db_partition_stats 
                WHERE index_id IN (0,1)";
            
            try
            {
                stats.TotalRows = await connection.QuerySingleOrDefaultAsync<long?>(rowCountSql) ?? 0;
            }
            catch
            {
                // در صورت عدم دسترسی به sys.dm_db_partition_stats
                stats.TotalRows = 0;
            }
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get database stats on {connectionStringName}");
            throw new DbConnectorException($"Failed to get database stats: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// پشتیبانی از چند پایگاه داده
    /// </summary>
    public async Task<DatabaseComparisonResult> CompareDatabasesAsync(string sourceConnectionString, string targetConnectionString)
    {
        try
        {
            var result = new DatabaseComparisonResult();
            
            // دریافت جداول منبع
            var sourceTables = await GetTablesAsync(sourceConnectionString);
            var targetTables = await GetTablesAsync(targetConnectionString);
            
            // مقایسه جداول
            result.MissingInTarget = sourceTables.Except(targetTables).ToList();
            result.ExtraInTarget = targetTables.Except(sourceTables).ToList();
            
            // مقایسه ساختار جداول مشترک
            var commonTables = sourceTables.Intersect(targetTables);
            foreach (var table in commonTables)
            {
                var sourceSchema = await GetTableSchemaAsync(sourceConnectionString, table);
                var targetSchema = await GetTableSchemaAsync(targetConnectionString, table);
                
                if (!AreSchemasEqual(sourceSchema, targetSchema))
                {
                    result.SchemaDifferences.Add(table);
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to compare databases {sourceConnectionString} and {targetConnectionString}");
            throw new DbConnectorException($"Failed to compare databases: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// مقایسه ساختار جداول
    /// </summary>
    private bool AreSchemasEqual(TableSchema source, TableSchema target)
    {
        if (source.Columns.Count != target.Columns.Count)
            return false;

        for (int i = 0; i < source.Columns.Count; i++)
        {
            if (source.Columns[i].ColumnName != target.Columns[i].ColumnName ||
                source.Columns[i].DataType != target.Columns[i].DataType ||
                source.Columns[i].IsNullable != target.Columns[i].IsNullable)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// استثناهای اتصال پایگاه داده
/// </summary>
public class DbConnectorException : Exception
{
    public DbConnectorException(string message) : base(message) { }
    
    public DbConnectorException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// اطلاعات ستون
/// </summary>
public class ColumnSchema
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public string IsNullable { get; set; }
    public string ColumnDefault { get; set; }
    public int? MaxLength { get; set; }
    public byte? NumericPrecision { get; set; }
    public int? NumericScale { get; set; }
}

/// <summary>
/// اطلاعات ساختار جدول
/// </summary>
public class TableSchema
{
    public string TableName { get; set; }
    public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
}

/// <summary>
/// آمار پایگاه داده
/// </summary>
public class DatabaseStats
{
    public int TableCount { get; set; }
    public long TotalRows { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// نتیجه مقایسه پایگاه‌های داده
/// </summary>
public class DatabaseComparisonResult
{
    public List<string> MissingInTarget { get; set; } = new List<string>();
    public List<string> ExtraInTarget { get; set; } = new List<string>();
    public List<string> SchemaDifferences { get; set; } = new List<string>();
    public bool AreIdentical => MissingInTarget.Count == 0 && ExtraInTarget.Count == 0 && SchemaDifferences.Count == 0;
}