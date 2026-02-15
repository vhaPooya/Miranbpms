namespace Automation.Core.Abstractions;

/// <summary>
/// قرارداد پایه ریپازیتوری برای دامنه‌های استاتیک (EF Core).
/// دامنه‌های داینامیک (FormBuilder, Workflow) از Dapper/SP استفاده می‌کنند.
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
