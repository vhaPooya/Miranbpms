namespace Automation.Core.Entities;

/// <summary>
/// Connection Strings
/// </summary>
public class ConnectionString : BaseEntity
{
    /// <summary>
    /// نام Connection String
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection String
    /// </summary>
    public string ConnectionStringValue { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع دیتابیس
    /// </summary>
    public string DatabaseType { get; set; } = "SqlServer"; // SqlServer, Oracle, MySQL, PostgreSQL
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



