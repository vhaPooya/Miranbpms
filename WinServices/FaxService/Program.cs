using Automation.FaxService;
using Automation.FaxService.Configuration;
using Automation.FaxService.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "Automation.FaxService");

builder.Services.Configure<FaxOptions>(builder.Configuration.GetSection(FaxOptions.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IFaxAgentService, FaxAgentService>();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

var faxOpts = app.Services.GetRequiredService<IOptions<FaxOptions>>().Value;

// سرو کردن فایل PDF از Outbox برای Twilio (BaseUrlForMedia → /faxfiles/{fileName})
app.MapGet("/faxfiles/{fileName}", (string fileName) =>
{
    var outbox = Path.GetFullPath(faxOpts.OutboxPath);
    var path = Path.Combine(outbox, fileName);
    if (!Path.GetFullPath(path).StartsWith(outbox, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
        return Results.NotFound();
    return Results.File(path, "application/pdf", fileName);
});

// وب‌هوک Twilio برای فکس دریافتی - Twilio ارسال می‌کند: MediaUrl برای دانلود PDF
app.MapPost("/fax/incoming", async (HttpRequest req, IFaxAgentService faxAgent, CancellationToken ct) =>
{
    var form = await req.ReadFormAsync(ct);
    var from = form["From"].ToString() ?? "";
    var sid = form["FaxSid"].ToString() ?? "";
    var mediaUrl = form["MediaUrl"].ToString();
    if (string.IsNullOrEmpty(mediaUrl))
        return Results.BadRequest("MediaUrl missing");
    using var http = new HttpClient();
    var pdfStream = await http.GetStreamAsync(mediaUrl, ct);
    await faxAgent.SaveIncomingFaxAsync(pdfStream, from, sid, ct);
    return Results.Ok();
});

app.Run();
