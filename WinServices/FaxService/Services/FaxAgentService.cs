using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Automation.FaxService.Configuration;
using Automation.FaxService.Models;
using Microsoft.Extensions.Options;

namespace Automation.FaxService.Services;

public class FaxAgentService : IFaxAgentService
{
    private readonly FaxOptions _options;
    private readonly ILogger<FaxAgentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string TwilioFaxApi = "https://fax.twilio.com/v1/Faxes";

    public FaxAgentService(
        IOptions<FaxOptions> options,
        ILogger<FaxAgentService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendAsync(OutboxFaxRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.SendEnabled)
        {
            _logger.LogWarning("ارسال فکس غیرفعال است.");
            return;
        }

        if (string.IsNullOrEmpty(_options.TwilioAccountSid) || string.IsNullOrEmpty(_options.TwilioAuthToken))
        {
            _logger.LogWarning("تنظیمات Twilio ناقص است.");
            return;
        }

        string mediaUrl = request.MediaUrl ?? "";
        if (string.IsNullOrEmpty(mediaUrl) && !string.IsNullOrEmpty(request.FileName) && !string.IsNullOrEmpty(_options.BaseUrlForMedia))
        {
            var baseUrl = _options.BaseUrlForMedia.TrimEnd('/');
            mediaUrl = $"{baseUrl}/{Uri.EscapeDataString(request.FileName)}";
        }

        if (string.IsNullOrEmpty(mediaUrl))
        {
            _logger.LogWarning("برای ارسال فکس باید MediaUrl یا FileName به همراه BaseUrlForMedia تنظیم شود.");
            return;
        }

        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.TwilioAccountSid}:{_options.TwilioAuthToken}"));
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        var form = new Dictionary<string, string>
        {
            ["To"] = request.To,
            ["From"] = _options.TwilioFromNumber ?? "",
            ["MediaUrl"] = mediaUrl
        };
        using var content = new FormUrlEncodedContent(form);
        var response = await client.PostAsync(TwilioFaxApi, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var sid = doc.RootElement.TryGetProperty("sid", out var s) ? s.GetString() : "";
        _logger.LogInformation("فکس ارسال شد. Sid: {Sid}, To: {To}, ReferenceId: {Ref}", sid, request.To, request.ReferenceId);
    }

    public async Task ProcessOutboxAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.SendEnabled) return;

        var dir = new DirectoryInfo(Path.GetFullPath(_options.OutboxPath));
        if (!dir.Exists)
        {
            dir.Create();
            return;
        }

        var files = dir.GetFiles("*.fax.json", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var json = await File.ReadAllTextAsync(file.FullName, cancellationToken);
                var request = JsonSerializer.Deserialize<OutboxFaxRequest>(json, JsonOptions);
                if (request == null) continue;
                await SendAsync(request, cancellationToken);
                File.Delete(file.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش فایل Outbox فکس: {File}", file.FullName);
            }
        }
    }

    public async Task SaveIncomingFaxAsync(Stream pdfStream, string fromNumber, string sid, CancellationToken cancellationToken = default)
    {
        var inboxDir = Path.GetFullPath(_options.InboxPath);
        Directory.CreateDirectory(inboxDir);
        var safeName = $"fax_{sid}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var path = Path.Combine(inboxDir, safeName);
        await using var fs = File.Create(path);
        await pdfStream.CopyToAsync(fs, cancellationToken);
        _logger.LogInformation("فکس دریافتی ذخیره شد. From: {From}, Sid: {Sid}, File: {File}", fromNumber, sid, path);
    }
}
