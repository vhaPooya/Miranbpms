using System.Text.Json;
using Automation.EmailService.Configuration;
using Automation.EmailService.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Automation.EmailService.Services;

public class EmailAgentService : IEmailAgentService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailAgentService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EmailAgentService(IOptions<EmailOptions> options, ILogger<EmailAgentService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(OutboxEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.SendEnabled)
        {
            _logger.LogWarning("ارسال ایمیل غیرفعال است.");
            return;
        }

        var smtp = _options.Smtp;
        if (string.IsNullOrEmpty(smtp.Host) || string.IsNullOrEmpty(smtp.UserName))
        {
            _logger.LogWarning("تنظیمات SMTP ناقص است.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp.DefaultFromName ?? smtp.UserName, smtp.DefaultFromAddress ?? smtp.UserName));
        message.To.Add(MailboxAddress.Parse(request.To));
        if (!string.IsNullOrWhiteSpace(request.Cc))
            foreach (var a in request.Cc.Split(';', ','))
                if (!string.IsNullOrWhiteSpace(a)) message.Cc.Add(MailboxAddress.Parse(a.Trim()));
        if (!string.IsNullOrWhiteSpace(request.Bcc))
            foreach (var a in request.Bcc.Split(';', ','))
                if (!string.IsNullOrWhiteSpace(a)) message.Bcc.Add(MailboxAddress.Parse(a.Trim()));
        message.Subject = request.Subject;

        var builder = new BodyBuilder();
        if (request.IsBodyHtml)
            builder.HtmlBody = request.Body;
        else
            builder.TextBody = request.Body;

        string? outboxDir = Path.GetFullPath(_options.OutboxPath);
        if (request.AttachmentPaths != null)
        {
            foreach (var path in request.AttachmentPaths)
            {
                var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(outboxDir, path);
                if (File.Exists(fullPath))
                    builder.Attachments.Add(Path.GetFileName(fullPath), File.ReadAllBytes(fullPath));
                else
                    _logger.LogWarning("فایل پیوست یافت نشد: {Path}", fullPath);
            }
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp.Host, smtp.Port, smtp.UseSsl, cancellationToken);
        await client.AuthenticateAsync(smtp.UserName, smtp.Password ?? "", cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        _logger.LogInformation("ایمیل با موفقیت ارسال شد. To: {To}, ReferenceId: {Ref}", request.To, request.ReferenceId);
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

        var files = dir.GetFiles("*.email.json", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var json = await File.ReadAllTextAsync(file.FullName, cancellationToken);
                var request = JsonSerializer.Deserialize<OutboxEmailRequest>(json, JsonOptions);
                if (request == null) continue;
                await SendAsync(request, cancellationToken);
                File.Delete(file.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش فایل Outbox: {File}", file.FullName);
            }
        }
    }

    public async Task ReceiveAndStoreAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.ReceiveEnabled) return;

        var imap = _options.Imap;
        if (string.IsNullOrEmpty(imap.Host) || string.IsNullOrEmpty(imap.UserName))
        {
            _logger.LogWarning("تنظیمات IMAP ناقص است.");
            return;
        }

        var inboxDir = Path.GetFullPath(_options.InboxPath);
        Directory.CreateDirectory(inboxDir);

        using var client = new ImapClient();
        await client.ConnectAsync(imap.Host, imap.Port, imap.UseSsl, cancellationToken);
        await client.AuthenticateAsync(imap.UserName, imap.Password ?? "", cancellationToken);
        var inbox = client.Inbox;
        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, cancellationToken);

        var uids = await inbox.SearchAsync(SearchQuery.All, cancellationToken);
        var existing = new HashSet<string>();
        foreach (var f in new DirectoryInfo(inboxDir).GetFiles("*.eml"))
            existing.Add(Path.GetFileNameWithoutExtension(f.Name));

        foreach (var uid in uids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = uid.Id.ToString();
            if (existing.Contains(id)) continue;

            try
            {
                var message = await inbox.GetMessageAsync(uid, cancellationToken);
                var safeName = $"{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.eml";
                var emlPath = Path.Combine(inboxDir, safeName);
                await message.WriteToAsync(emlPath, cancellationToken);

                var metaPath = Path.Combine(inboxDir, Path.ChangeExtension(safeName, ".json"));
                var meta = new
                {
                    MessageId = id,
                    From = message.From.ToString(),
                    To = message.To.ToString(),
                    Subject = message.Subject,
                    Date = message.Date,
                    EmlFile = safeName
                };
                await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(meta), cancellationToken);
                existing.Add(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ذخیره ایمیل UID {Uid}", uid);
            }
        }

        await client.DisconnectAsync(true, cancellationToken);
        _logger.LogInformation("بررسی صندوق ورودی انجام شد. تعداد: {Count}", uids.Count);
    }
}
