// Automation.Web.Host — نقطه ورود اصلی (طبق پرامپت: فقط startup، بارگذاری ماژول، پیکربندی سراسری؛ بدون منطق کسب‌وکار)
using Automation.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Automation.Web.Hubs;
using Automation.Web.Host.Services;
using Automation.Infrastructure;
using Automation.Module.FormBuilder;
using Automation.Module.Workflow;
using Automation.Module.Secretariat;

var builder = WebApplication.CreateBuilder(args);

// Load modules (FormBuilder, Workflow, Secretariat)
builder.Services.AddModules(builder.Configuration,
    typeof(FormBuilderModule),
    typeof(WorkflowModule),
    typeof(SecretariatModule));

// MediatR for inter-module events
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(Automation.Core.Events.LetterCreatedEvent).Assembly,
    typeof(Automation.Module.Workflow.Handlers.LetterCreatedEventHandler).Assembly));

// Controllers: Host + Web (Views/Account/Home) + Module assemblies (Module Loader)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<Automation.Web.Filters.CurrentContextViewBagFilter>();
})
.AddApplicationPart(typeof(Automation.Web.Controllers.HomeController).Assembly) // Web: Home, Account, ...
.AddApplicationPart(typeof(FormBuilderModule).Assembly)
.AddApplicationPart(typeof(WorkflowModule).Assembly)
.AddApplicationPart(typeof(SecretariatModule).Assembly);

builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddHttpContextAccessor();

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(conn));
builder.Services.AddDbContext<CoreDbContext>(options => options.UseSqlServer(conn));
builder.Services.AddDbContext<AutomationDbContext>(options => options.UseSqlServer(conn));

builder.Services.AddScoped<Automation.Core.Interfaces.IPermissionService, Automation.Infrastructure.Services.PermissionService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IDocumentService, Automation.Infrastructure.Services.DocumentService>();
builder.Services.AddScoped<Automation.Core.Interfaces.ICabinetService, Automation.Infrastructure.Services.CabinetService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IArchiveService, Automation.Infrastructure.Services.ArchiveService>();
builder.Services.AddScoped<Automation.Core.Interfaces.ISearchService, Automation.Infrastructure.Services.SearchService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IFileUploadService, Automation.Infrastructure.Services.FileUploadService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IEmailService, Automation.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IFaxService, Automation.Infrastructure.Services.FaxService>();
builder.Services.AddScoped<Automation.Core.Interfaces.ISessionService, Automation.Web.Services.SessionService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<Automation.Core.Interfaces.ICacheService, Automation.Infrastructure.Services.MemoryCacheService>();
builder.Services.AddScoped<Automation.Infrastructure.Services.DocumentValidationService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IUserContextService, Automation.Infrastructure.Services.UserContextService>();
builder.Services.AddScoped<Automation.Core.Interfaces.ICurrentContext, Automation.Infrastructure.Services.CurrentContext>();
builder.Services.AddScoped<Automation.Core.Interfaces.ICurrentContextSetter, Automation.Infrastructure.Services.CurrentContextSetter>();
builder.Services.AddScoped<Automation.Web.Filters.CurrentContextViewBagFilter>();
builder.Services.AddScoped<Automation.Core.Interfaces.IDapperService, Automation.Infrastructure.Services.DapperService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IFormButtonService, Automation.Infrastructure.Services.FormButtonService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IPasswordHasher, Automation.Infrastructure.Services.PasswordHasher>();
builder.Services.AddIntegrationServices();
builder.Services.AddExternalHttpClients(builder.Configuration);
builder.Services.AddPerformanceAndScalabilityServices(builder.Configuration);

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
    var context = scope.ServiceProvider.GetRequiredService<AutomationDbContext>();
    await context.Database.EnsureCreatedAsync();
    await SeedData.SeedAllAsync(identityContext, coreContext, context);
    Console.WriteLine("Database initialized successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization error: {ex.Message}");
    if (!app.Environment.IsDevelopment()) throw;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<Automation.Web.Hubs.NotificationHub>("/notificationhub");

Console.WriteLine("Automation.Web.Host starting...");
app.Run();
