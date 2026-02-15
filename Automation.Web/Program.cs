using Automation.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Automation.Web.Hubs;
using Automation.Web.Services;
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

// MediatR for inter-module events (e.g. LetterCreated -> Workflow)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(Automation.Core.Events.LetterCreatedEvent).Assembly,
    typeof(Automation.Module.Workflow.Handlers.LetterCreatedEventHandler).Assembly));

// Add services to the container. Load Controllers/Views from module assemblies (Module Loader)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<Automation.Web.Filters.CurrentContextViewBagFilter>();
})
.AddApplicationPart(typeof(FormBuilderModule).Assembly)
.AddApplicationPart(typeof(WorkflowModule).Assembly)
.AddApplicationPart(typeof(SecretariatModule).Assembly);

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Authentication
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

// Add HttpContextAccessor for Session Service
builder.Services.AddHttpContextAccessor();

// Configure Database — Bounded contexts: Identity (EF), Core (EF), Automation (EF for metadata; Dapper/SP for execution)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<Automation.Infrastructure.Data.IdentityDbContext>(options =>
    options.UseSqlServer(conn));
builder.Services.AddDbContext<Automation.Infrastructure.Data.CoreDbContext>(options =>
    options.UseSqlServer(conn));
builder.Services.AddDbContext<AutomationDbContext>(options =>
    options.UseSqlServer(conn));

// Form services are registered by FormBuilderModule

// Register Permission & Document Services
builder.Services.AddScoped<Automation.Core.Interfaces.IPermissionService, Automation.Infrastructure.Services.PermissionService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IDocumentService, Automation.Infrastructure.Services.DocumentService>();

// Register Cabinet, Archive & Search Services
builder.Services.AddScoped<Automation.Core.Interfaces.ICabinetService, Automation.Infrastructure.Services.CabinetService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IArchiveService, Automation.Infrastructure.Services.ArchiveService>();
builder.Services.AddScoped<Automation.Core.Interfaces.ISearchService, Automation.Infrastructure.Services.SearchService>();

// Register File Upload, Email & Fax Services
builder.Services.AddScoped<Automation.Core.Interfaces.IFileUploadService, Automation.Infrastructure.Services.FileUploadService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IEmailService, Automation.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<Automation.Core.Interfaces.IFaxService, Automation.Infrastructure.Services.FaxService>();

// Register Session Service
builder.Services.AddScoped<Automation.Core.Interfaces.ISessionService, Automation.Web.Services.SessionService>();

// Register Cache Service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<Automation.Core.Interfaces.ICacheService, Automation.Infrastructure.Services.MemoryCacheService>();

// Register Validation Service
builder.Services.AddScoped<Automation.Infrastructure.Services.DocumentValidationService>();

// Register User Context Service
builder.Services.AddScoped<Automation.Core.Interfaces.IUserContextService, Automation.Infrastructure.Services.UserContextService>();

// Register Current Context (OUserId, OPosId, OFEIC, OFEC) for BPMS-wide access
builder.Services.AddScoped<Automation.Core.Interfaces.ICurrentContext, Automation.Infrastructure.Services.CurrentContext>();
builder.Services.AddScoped<Automation.Core.Interfaces.ICurrentContextSetter, Automation.Infrastructure.Services.CurrentContextSetter>();
builder.Services.AddScoped<Automation.Web.Filters.CurrentContextViewBagFilter>();

// Register Dapper Service
builder.Services.AddScoped<Automation.Core.Interfaces.IDapperService, Automation.Infrastructure.Services.DapperService>();

// Register Form Button Service
builder.Services.AddScoped<Automation.Core.Interfaces.IFormButtonService, Automation.Infrastructure.Services.FormButtonService>();

// Register Password Hasher
builder.Services.AddScoped<Automation.Core.Interfaces.IPasswordHasher, Automation.Infrastructure.Services.PasswordHasher>();

// Register Integration Services
builder.Services.AddIntegrationServices();
builder.Services.AddExternalHttpClients(builder.Configuration);

// Register Performance and Scalability Services
builder.Services.AddPerformanceAndScalabilityServices(builder.Configuration);

var app = builder.Build();

// Seed database with error handling
try
{
    using var scope = app.Services.CreateScope();
    var identityContext = scope.ServiceProvider.GetRequiredService<Automation.Infrastructure.Data.IdentityDbContext>();
    var coreContext = scope.ServiceProvider.GetRequiredService<Automation.Infrastructure.Data.CoreDbContext>();
    var context = scope.ServiceProvider.GetRequiredService<AutomationDbContext>();
    await context.Database.EnsureCreatedAsync();
    await SeedData.SeedAllAsync(identityContext, coreContext, context);
    Console.WriteLine("Database initialized successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization error: {ex.Message}");
    // Continue running even if DB init fails in dev mode
    if (!app.Environment.IsDevelopment())
        throw;
}

// Configure the HTTP request pipeline.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<Automation.Web.Hubs.NotificationHub>("/notificationhub");

Console.WriteLine("Starting web server on http://localhost:5000...");
app.Run();


