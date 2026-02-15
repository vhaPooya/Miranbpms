using Automation.Core.Abstractions;
using Automation.Core.Interfaces;
using Automation.Infrastructure.Services;
using Automation.Module.FormBuilder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Module.FormBuilder;

/// <summary>
/// ماژول فرم‌ساز و داده‌های داینامیک فرم.
/// </summary>
public class FormBuilderModule : IModule
{
    public string Name => "FormBuilder";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFormSchemaService, FormSchemaService>();
        services.AddScoped<IFormTableService, FormTableService>();
        services.AddScoped<IFormXmlService, FormXmlService>();
        services.AddScoped<IWordDocumentService, WordDocumentService>();
        services.AddScoped<IDynamicTableGeneratorService, DynamicTableGeneratorService>();
    }
}
