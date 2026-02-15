using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Core.Abstractions;

/// <summary>
/// قرارداد ماژول در معماری Modular Monolith.
/// هر ماژول (FormBuilder, Workflow, Secretariat) این اینترفیس را پیاده‌سازی می‌کند.
/// </summary>
public interface IModule
{
    /// <summary>
    /// نام ماژول (یکتا)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// ثبت سرویس‌ها و وابستگی‌های ماژول در DI.
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
