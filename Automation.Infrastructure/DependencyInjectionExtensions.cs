using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Automation.Infrastructure.Data;
using Automation.Infrastructure.Services.Integration.ThirdPartyConnectors;
using Automation.Infrastructure.Services.Integration.ApiGateway;
using Automation.Infrastructure.Services.Integration.WebServices;
using Automation.Infrastructure.Services.Integration.Database;
using Automation.Infrastructure.Services.Caching;
using Automation.Infrastructure.Services.Performance;
using StackExchange.Redis;

namespace Automation.Infrastructure;

/// <summary>
/// افزودن سرویس‌های ادغام به DI Container
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// ثبت سه Context محدود: Identity (کاربر/نقش/سازمان)، Core (دبیرخانه/تنظیمات پایه)، Automation (فرم/سند/گردش/گزارش).
    /// برای داده‌های پویا (اجرای گردش، داده فرم) از Dapper/SP استفاده شود.
    /// </summary>
    public static IServiceCollection AddBoundedContexts(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDbContext<CoreDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDbContext<AutomationDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }

    /// <summary>
    /// افزودن سرویس‌های ادغام
    /// </summary>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        // سرویس‌های اتصال به سیستم‌های خارجی
        services.AddScoped<ErpConnector>();
        services.AddScoped<CrmConnector>();
        
        // سرویس دروازه API
        services.AddHttpClient();
        services.AddScoped<ApiGatewayService>();
        
        // سرویس‌های وب
        services.AddScoped<WebServiceClient>();
        services.AddScoped<SpecificWebServiceClients>();
        
        // سرویس اتصال پایگاه داده
        services.AddScoped<DbConnectorService>();
        
        return services;
    }

    /// <summary>
    /// افزودن کلاینت‌های HTTP برای سرویس‌های خارجی
    /// </summary>
    public static IServiceCollection AddExternalHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        // کلاینت HTTP برای ERP
        services.AddHttpClient("ERP", client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:ERP:BaseUrl"] ?? "https://erp.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // کلاینت HTTP برای CRM
        services.AddHttpClient("CRM", client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:CRM:BaseUrl"] ?? "https://crm.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // کلاینت HTTP برای سرویس پرداخت
        services.AddHttpClient("PaymentService", client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:Payment:BaseUrl"] ?? "https://payment.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // کلاینت HTTP برای سرویس پیامک
        services.AddHttpClient("SMSService", client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:SMS:BaseUrl"] ?? "https://sms.example.com");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        
        return services;
    }

    /// <summary>
    /// افزودن سرویس‌های عملکرد و مقیاس‌پذیری
    /// </summary>
    public static IServiceCollection AddPerformanceAndScalabilityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // سرویس کش Redis
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connectionString);
        });
        services.AddScoped<Automation.Core.Interfaces.ICacheService, RedisCacheService>();
        services.AddScoped<CacheManager>();

        // سرویس بهینه‌سازی دیتابیس
        services.AddScoped<DatabaseOptimizer>();

        // سرویس توزیع بار
        services.AddSingleton<LoadBalancer>();
        services.AddScoped<ServiceLoadBalancer>();

        // سرویس مانیتورینگ عملکرد
        services.AddSingleton<PerformanceMonitor>();
        services.AddSingleton<RealTimePerformanceCollector>();
        
        // سرویس سلامت
        services.AddHealthChecks()
            .AddCheck<EbpmsHealthCheck>("ebpms_health_check");

        return services;
    }
}