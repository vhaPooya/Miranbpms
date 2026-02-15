using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس Dapper برای دسترسی به دیتابیس با بهینه‌سازی سرعت
/// </summary>
public class DapperService : IDapperService
{
    private readonly string _connectionString;

    public DapperService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public string GetConnectionString() => _connectionString;

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = CreateConnection();
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }
        return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = CreateConnection();
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var connection = CreateConnection();
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
        }
        return await connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
    }

    public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        var connection = CreateConnection();

        var cmd = new CommandDefinition(
            sql,
            param,
            transaction,
            commandTimeout,
            commandType,
            CommandFlags.None);

        return await connection.QueryMultipleAsync(cmd);
    }
}



