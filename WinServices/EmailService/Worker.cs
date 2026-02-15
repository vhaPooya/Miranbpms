using Automation.EmailService.Configuration;
using Automation.EmailService.Services;
using Microsoft.Extensions.Options;

namespace Automation.EmailService;

public class Worker : BackgroundService
{
    private readonly IEmailAgentService _emailAgent;
    private readonly ILogger<Worker> _logger;
    private readonly int _pollIntervalSeconds;

    public Worker(
        IEmailAgentService emailAgent,
        IOptions<EmailOptions> options,
        ILogger<Worker> logger)
    {
        _emailAgent = emailAgent;
        _logger = logger;
        _pollIntervalSeconds = Math.Max(30, options.Value.PollIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("سرویس ایمیل اتوماسیون راه‌اندازی شد.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _emailAgent.ProcessOutboxAsync(stoppingToken);
                await _emailAgent.ReceiveAndStoreAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در چرخه پردازش ایمیل");
            }

            await Task.Delay(TimeSpan.FromSeconds(_pollIntervalSeconds), stoppingToken);
        }
    }
}
