using Automation.EmailService;
using Automation.EmailService.Configuration;
using Automation.EmailService.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "Automation.EmailService");

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddSingleton<IEmailAgentService, EmailAgentService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
