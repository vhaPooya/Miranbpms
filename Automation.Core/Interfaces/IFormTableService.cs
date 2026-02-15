using Automation.Core.Entities;

namespace Automation.Core.Interfaces;

/// <summary>
/// Service for creating and managing database tables for forms
/// </summary>
public interface IFormTableService
{
    /// <summary>
    /// Create database table for a form based on its fields
    /// </summary>
    Task<bool> CreateFormTableAsync(Form form, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update database table structure when form fields change
    /// </summary>
    Task<bool> UpdateFormTableAsync(Form form, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Drop form table
    /// </summary>
    Task<bool> DropFormTableAsync(string tableName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get table schema
    /// </summary>
    Task<string> GetTableSchemaAsync(string tableName, CancellationToken cancellationToken = default);
}




