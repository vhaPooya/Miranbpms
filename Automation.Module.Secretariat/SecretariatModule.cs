using Automation.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Module.Secretariat;

/// <summary>
/// ماژول دبیرخانه (مدارک، نامه‌ها، ارجاعات).
/// </summary>
public class SecretariatModule : IModule
{
    public string Name => "Secretariat";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Secretariat-specific services
    }
}
