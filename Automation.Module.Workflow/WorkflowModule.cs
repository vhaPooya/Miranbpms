using Automation.Core.Abstractions;
using Automation.Module.Workflow.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Module.Workflow;

/// <summary>
/// ماژول فرآیند و موتور گردش کار (اجرای منطق در دیتابیس با SP).
/// </summary>
public class WorkflowModule : IModule
{
    public string Name => "Workflow";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IWorkflowEngineSqlService, WorkflowEngineSqlService>();
    }
}
