using System.Data;
using Dapper;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس Dapper برای دسترسی به دیتابیس با بهینه‌سازی سرعت
/// </summary>
public interface IDapperService
{
    /// <summary>
    /// دریافت Connection String
    /// </summary>
    string GetConnectionString();
    
    /// <summary>
    /// ایجاد Connection جدید
    /// </summary>
    IDbConnection CreateConnection();
    
    /// <summary>
    /// اجرای کوئری و بازگشت لیست
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    
    /// <summary>
    /// اجرای کوئری و بازگشت یک آیتم
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    
    /// <summary>
    /// اجرای دستور (Insert, Update, Delete)
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    
    /// <summary>
    /// اجرای کوئری با چند نتیجه
    /// </summary>
    Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
}



