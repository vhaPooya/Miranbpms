using Automation.FaxService.Configuration;
using Automation.FaxService.Services;
using Microsoft.Extensions.Options;

namespace Automation.FaxService;

public class Worker : BackgroundService
{
    private readonly IFaxAgentService _faxAgent;
    private readonly ILogger<Worker> _logger;
    private readonly int _pollIntervalSeconds;

    public Worker(
        IFaxAgentService faxAgent,
        IOptions<FaxOptions> options,
        ILogger<Worker> logger)
    {
        _faxAgent = faxAgent;
        _logger = logger;
        _pollIntervalSeconds = Math.Max(30, options.Value.PollIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("سرویس فکس اتوماسیون راه‌اندازی شد.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _faxAgent.ProcessOutboxAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در چرخه پردازش فکس");
            }

            await Task.Delay(TimeSpan.FromSeconds(_pollIntervalSeconds), stoppingToken);
        }
    }
}
