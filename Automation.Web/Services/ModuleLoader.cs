using Automation.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Automation.Web.Services;

/// <summary>
/// بارگذاری ماژول‌ها با Reflection و ثبت سرویس‌های آن‌ها در DI.
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// اسکن اسمبلی‌ها برای یافتن انواعی که IModule را پیاده‌سازی می‌کنند و فراخوانی ConfigureServices آن‌ها.
    /// </summary>
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[]? assembliesToScan = null)
    {
        var assemblies = assembliesToScan ?? new[] { Assembly.GetEntryAssembly()! };
        if (assemblies.Length == 0 || assemblies[0] == null)
            return services;

        var moduleTypes = new List<Type>();
        foreach (var asm in assemblies.Where(a => a != null))
        {
            try
            {
                var types = asm.GetExportedTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });
                moduleTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException) { /* skip */ }
        }

        foreach (var type in moduleTypes.Distinct())
        {
            try
            {
                if (Activator.CreateInstance(type) is IModule module)
                {
                    module.ConfigureServices(services, configuration);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ModuleLoader: Failed to load module {type.Name}: {ex.Message}");
            }
        }

        return services;
    }

    /// <summary>
    /// ثبت ماژول‌های مشخص‌شده با نوع (بدون اسکن اسمبلی).
    /// </summary>
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Type[] moduleTypes)
    {
        foreach (var type in moduleTypes)
        {
            if (!typeof(IModule).IsAssignableFrom(type))
                continue;
            try
            {
                if (Activator.CreateInstance(type) is IModule module)
                    module.ConfigureServices(services, configuration);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ModuleLoader: Failed to load module {type.Name}: {ex.Message}");
            }
        }

        return services;
    }
}
