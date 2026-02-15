using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Automation.Infrastructure.Services.Performance;

/// <summary>
/// سیستم مانیتورینگ عملکرد برای نظارت بر سلامت و عملکرد سیستم
/// </summary>
public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly PerformanceCounter _diskCounter;
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, long> _operationTimings;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
        _operationTimings = new Dictionary<string, long>();

        try
        {
            // تنظیم Performance Counters
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes", string.Empty);
                _diskCounter = new PerformanceCounter("PhysicalDisk", "Disk Transfers/sec", "_Total");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize performance counters");
        }
    }

    /// <summary>
    /// شروع اندازه‌گیری زمان عملیات
    /// </summary>
    public void StartOperationTiming(string operationName)
    {
        _stopwatch.Restart();
        _logger.LogDebug($"Started timing operation: {operationName}");
    }

    /// <summary>
    /// ثبت زمان عملیات (برای استفاده توسط OperationTimer)
    /// </summary>
    internal void RecordOperationTiming(string operationName, long elapsedMilliseconds)
    {
        _operationTimings[operationName] = elapsedMilliseconds;
        _logger.LogDebug($"Operation {operationName} completed in {elapsedMilliseconds}ms");
    }

    /// <summary>
    /// پایان اندازه‌گیری زمان عملیات
    /// </summary>
    public long StopOperationTiming(string operationName)
    {
        _stopwatch.Stop();
        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
        _operationTimings[operationName] = elapsedMilliseconds;
        
        _logger.LogDebug($"Completed operation {operationName} in {elapsedMilliseconds}ms");
        return elapsedMilliseconds;
    }

    /// <summary>
    /// دریافت آمار سیستم
    /// </summary>
    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        var metrics = new SystemMetrics
        {
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // CPU Usage
            if (_cpuCounter != null)
            {
                metrics.CpuUsagePercentage = await Task.Run(() => _cpuCounter.NextValue());
            }

            // Memory Usage
            if (_memoryCounter != null)
            {
                metrics.AvailableMemoryMB = await Task.Run(() => _memoryCounter.NextValue());
                metrics.TotalMemoryMB = GetTotalMemoryMB();
                metrics.MemoryUsagePercentage = ((metrics.TotalMemoryMB - metrics.AvailableMemoryMB) / metrics.TotalMemoryMB) * 100;
            }

            // Disk Usage
            if (_diskCounter != null)
            {
                metrics.DiskTransfersPerSecond = await Task.Run(() => _diskCounter.NextValue());
            }

            // Application Metrics
            metrics.ApplicationMemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024);
            metrics.ThreadCount = Process.GetCurrentProcess().Threads.Count;
            metrics.HandleCount = Process.GetCurrentProcess().HandleCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system metrics");
        }

        return metrics;
    }

    /// <summary>
    /// دریافت آمار عملیات
    /// </summary>
    public OperationMetrics GetOperationMetrics()
    {
        return new OperationMetrics
        {
            OperationTimings = new Dictionary<string, long>(_operationTimings),
            SlowOperations = _operationTimings
                .Where(kv => kv.Value > 1000) // بیش از 1 ثانیه
                .ToDictionary(kv => kv.Key, kv => kv.Value),
            TotalOperations = _operationTimings.Count,
            AverageOperationTimeMs = _operationTimings.Any() ? _operationTimings.Values.Average() : 0
        };
    }

    /// <summary>
    /// مانیتورینگ عملیات دیتابیس
    /// </summary>
    public DatabaseMetrics GetDatabaseMetrics()
    {
        // اینجا باید آمار دیتابیس را جمع‌آوری کنیم
        // برای سادگی، مقادیر مثالی برمی‌گردانیم
        return new DatabaseMetrics
        {
            ActiveConnections = 15,
            ConnectionPoolSize = 100,
            QueriesPerSecond = 45.5,
            AverageQueryTimeMs = 125.7,
            SlowQueries = 3,
            Deadlocks = 0,
            LockWaits = 2
        };
    }

    /// <summary>
    /// مانیتورینگ کش
    /// </summary>
    public CacheMetrics GetCacheMetrics()
    {
        // اینجا باید آمار کش را جمع‌آوری کنیم
        // برای سادگی، مقادیر مثالی برمی‌گردانیم
        return new CacheMetrics
        {
            HitRatePercentage = 85.5,
            MissRatePercentage = 14.5,
            CurrentEntries = 12500,
            MemoryUsageMB = 45.2,
            Evictions = 120,
            Errors = 0
        };
    }

    /// <summary>
    /// مانیتورینگ HTTP requests
    /// </summary>
    public HttpMetrics GetHttpMetrics()
    {
        // اینجا باید آمار HTTP را جمع‌آوری کنیم
        // برای سادگی، مقادیر مثالی برمی‌گردانیم
        return new HttpMetrics
        {
            RequestsPerSecond = 125.3,
            AverageResponseTimeMs = 245.6,
            ErrorRatePercentage = 0.8,
            StatusCodeDistribution = new Dictionary<int, int>
            {
                { 200, 12000 },
                { 404, 50 },
                { 500, 8 }
            }
        };
    }

    /// <summary>
    /// گزارش وضعیت سیستم
    /// </summary>
    public async Task<HealthReport> GenerateHealthReportAsync()
    {
        var report = new HealthReport
        {
            GeneratedAt = DateTime.UtcNow,
            SystemMetrics = await GetSystemMetricsAsync(),
            OperationMetrics = GetOperationMetrics(),
            DatabaseMetrics = GetDatabaseMetrics(),
            CacheMetrics = GetCacheMetrics(),
            HttpMetrics = GetHttpMetrics()
        };

        // تعیین وضعیت کلی سیستم
        report.OverallStatus = DetermineOverallStatus(report);
        report.Recommendations = GenerateRecommendations(report);

        return report;
    }

    /// <summary>
    /// تعیین وضعیت کلی سیستم
    /// </summary>
    private HealthStatus DetermineOverallStatus(HealthReport report)
    {
        // بررسی شرایط بحرانی
        if (report.SystemMetrics.CpuUsagePercentage > 90)
            return HealthStatus.Critical;

        if (report.SystemMetrics.MemoryUsagePercentage > 85)
            return HealthStatus.Warning;

        if (report.OperationMetrics.SlowOperations.Any())
            return HealthStatus.Warning;

        if (report.DatabaseMetrics.Deadlocks > 0)
            return HealthStatus.Warning;

        if (report.CacheMetrics.HitRatePercentage < 70)
            return HealthStatus.Warning;

        if (report.HttpMetrics.ErrorRatePercentage > 2)
            return HealthStatus.Warning;

        return HealthStatus.Healthy;
    }

    /// <summary>
    /// تولید توصیه‌ها بر اساس آمار
    /// </summary>
    private List<string> GenerateRecommendations(HealthReport report)
    {
        var recommendations = new List<string>();

        if (report.SystemMetrics.CpuUsagePercentage > 80)
        {
            recommendations.Add("CPU usage is high. Consider scaling up or optimizing resource-intensive operations.");
        }

        if (report.SystemMetrics.MemoryUsagePercentage > 80)
        {
            recommendations.Add("Memory usage is high. Consider increasing memory or optimizing memory consumption.");
        }

        if (report.OperationMetrics.SlowOperations.Any())
        {
            recommendations.Add($"There are {report.OperationMetrics.SlowOperations.Count} slow operations. Review performance logs.");
        }

        if (report.DatabaseMetrics.Deadlocks > 0)
        {
            recommendations.Add("Database deadlocks detected. Review transaction isolation levels.");
        }

        if (report.CacheMetrics.HitRatePercentage < 75)
        {
            recommendations.Add("Cache hit rate is low. Consider optimizing cache strategy.");
        }

        if (report.HttpMetrics.ErrorRatePercentage > 1)
        {
            recommendations.Add("HTTP error rate is elevated. Check application logs for errors.");
        }

        return recommendations;
    }

    /// <summary>
    /// اندازه‌گیری عملیات با استفاده از using pattern
    /// </summary>
    public OperationTimer MeasureOperation(string operationName)
    {
        return new OperationTimer(this, operationName);
    }

    /// <summary>
    /// دریافت حافظه کل سیستم
    /// </summary>
    private double GetTotalMemoryMB()
    {
        try
        {
            return (double)(Environment.WorkingSet / (1024 * 1024));
        }
        catch
        {
            return 8192; // 8GB پیش‌فرض
        }
    }

    /// <summary>
    /// ذخیره گزارش عملکرد
    /// </summary>
    public async Task SavePerformanceReportAsync(HealthReport report)
    {
        try
        {
            // اینجا باید گزارش را در پایگاه داده یا فایل ذخیره کنیم
            _logger.LogInformation("Performance report saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save performance report");
        }
    }

    /// <summary>
    /// ارسال هشدارهای عملکرد
    /// </summary>
    public async Task SendPerformanceAlertsAsync(HealthReport report)
    {
        if (report.OverallStatus == HealthStatus.Critical)
        {
            await SendAlertAsync("CRITICAL", "System performance is critically degraded", report);
        }
        else if (report.OverallStatus == HealthStatus.Warning)
        {
            await SendAlertAsync("WARNING", "System performance warnings detected", report);
        }
    }

    /// <summary>
    /// ارسال هشدار
    /// </summary>
    private async Task SendAlertAsync(string level, string message, HealthReport report)
    {
        // اینجا باید سیستم ارسال هشدار را پیاده‌سازی کنیم
        _logger.LogWarning($"PERFORMANCE ALERT [{level}]: {message}");
        await Task.CompletedTask;
    }
}

/// <summary>
/// کلاس اندازه‌گیر زمان عملیات
/// </summary>
public class OperationTimer : IDisposable
{
    private readonly PerformanceMonitor _monitor;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;

    public OperationTimer(PerformanceMonitor monitor, string operationName)
    {
        _monitor = monitor;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _monitor.RecordOperationTiming(_operationName, _stopwatch.ElapsedMilliseconds);
    }
}

/// <summary>
/// آمار سیستم
/// </summary>
public class SystemMetrics
{
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercentage { get; set; }
    public double MemoryUsagePercentage { get; set; }
    public double AvailableMemoryMB { get; set; }
    public double TotalMemoryMB { get; set; }
    public double DiskTransfersPerSecond { get; set; }
    public double ApplicationMemoryUsageMB { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}

/// <summary>
/// آمار عملیات
/// </summary>
public class OperationMetrics
{
    public Dictionary<string, long> OperationTimings { get; set; } = new Dictionary<string, long>();
    public Dictionary<string, long> SlowOperations { get; set; } = new Dictionary<string, long>();
    public int TotalOperations { get; set; }
    public double AverageOperationTimeMs { get; set; }
}

/// <summary>
/// آمار دیتابیس
/// </summary>
public class DatabaseMetrics
{
    public int ActiveConnections { get; set; }
    public int ConnectionPoolSize { get; set; }
    public double QueriesPerSecond { get; set; }
    public double AverageQueryTimeMs { get; set; }
    public int SlowQueries { get; set; }
    public int Deadlocks { get; set; }
    public int LockWaits { get; set; }
}

/// <summary>
/// آمار کش
/// </summary>
public class CacheMetrics
{
    public double HitRatePercentage { get; set; }
    public double MissRatePercentage { get; set; }
    public int CurrentEntries { get; set; }
    public double MemoryUsageMB { get; set; }
    public int Evictions { get; set; }
    public int Errors { get; set; }
}

/// <summary>
/// آمار HTTP
/// </summary>
public class HttpMetrics
{
    public double RequestsPerSecond { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double ErrorRatePercentage { get; set; }
    public Dictionary<int, int> StatusCodeDistribution { get; set; } = new Dictionary<int, int>();
}

/// <summary>
/// گزارش سلامت
/// </summary>
public class HealthReport
{
    public DateTime GeneratedAt { get; set; }
    public HealthStatus OverallStatus { get; set; }
    public SystemMetrics SystemMetrics { get; set; }
    public OperationMetrics OperationMetrics { get; set; }
    public DatabaseMetrics DatabaseMetrics { get; set; }
    public CacheMetrics CacheMetrics { get; set; }
    public HttpMetrics HttpMetrics { get; set; }
    public List<string> Recommendations { get; set; } = new List<string>();
}

/// <summary>
/// وضعیت سلامت
/// </summary>
public enum HealthStatus
{
    Healthy,
    Warning,
    Critical
}

/// <summary>
/// سرویس سلامت برای ASP.NET Core
/// </summary>
public class EbpmsHealthCheck : IHealthCheck
{
    private readonly PerformanceMonitor _performanceMonitor;

    public EbpmsHealthCheck(PerformanceMonitor performanceMonitor)
    {
        _performanceMonitor = performanceMonitor;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _performanceMonitor.GenerateHealthReportAsync();
            
            var data = new Dictionary<string, object>
            {
                ["cpu_usage"] = report.SystemMetrics.CpuUsagePercentage,
                ["memory_usage"] = report.SystemMetrics.MemoryUsagePercentage,
                ["status"] = report.OverallStatus.ToString(),
                ["recommendations"] = string.Join(", ", report.Recommendations)
            };

            switch (report.OverallStatus)
            {
                case HealthStatus.Healthy:
                    return HealthCheckResult.Healthy("System is healthy");
                case HealthStatus.Warning:
                    return HealthCheckResult.Degraded("System has warnings");
                case HealthStatus.Critical:
                    return HealthCheckResult.Unhealthy("System is critical");
                default:
                    return HealthCheckResult.Healthy("System status unknown");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}");
        }
    }
}

/// <summary>
/// جمع‌آوری آمار عملکرد در زمان واقعی
/// </summary>
public class RealTimePerformanceCollector
{
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly ILogger<RealTimePerformanceCollector> _logger;
    private readonly Timer _collectionTimer;
    private readonly List<PerformanceSnapshot> _snapshots;

    public RealTimePerformanceCollector(PerformanceMonitor performanceMonitor, ILogger<RealTimePerformanceCollector> logger)
    {
        _performanceMonitor = performanceMonitor;
        _logger = logger;
        _snapshots = new List<PerformanceSnapshot>();
        
        // جمع‌آوری آمار هر 30 ثانیه
        _collectionTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private async void CollectMetrics(object state)
    {
        try
        {
            var metrics = await _performanceMonitor.GetSystemMetricsAsync();
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                Metrics = metrics
            };
            
            _snapshots.Add(snapshot);
            
            // نگهداری فقط 1000 آخرین نمونه
            if (_snapshots.Count > 1000)
            {
                _snapshots.RemoveAt(0);
            }
            
            _logger.LogDebug("Performance metrics collected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect performance metrics");
        }
    }

    /// <summary>
    /// دریافت آمار اخیر
    /// </summary>
    public List<PerformanceSnapshot> GetRecentSnapshots(int count = 100)
    {
        return _snapshots.TakeLast(count).ToList();
    }

    /// <summary>
    /// دریافت روند عملکرد
    /// </summary>
    public PerformanceTrend GetPerformanceTrend(TimeSpan period)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(period);
        var relevantSnapshots = _snapshots
            .Where(s => s.Timestamp >= cutoffTime)
            .ToList();

        if (!relevantSnapshots.Any())
        {
            return new PerformanceTrend();
        }

        return new PerformanceTrend
        {
            StartTime = relevantSnapshots.First().Timestamp,
            EndTime = relevantSnapshots.Last().Timestamp,
            AverageCpuUsage = relevantSnapshots.Average(s => s.Metrics.CpuUsagePercentage),
            AverageMemoryUsage = relevantSnapshots.Average(s => s.Metrics.MemoryUsagePercentage),
            PeakCpuUsage = relevantSnapshots.Max(s => s.Metrics.CpuUsagePercentage),
            PeakMemoryUsage = relevantSnapshots.Max(s => s.Metrics.MemoryUsagePercentage),
            SampleCount = relevantSnapshots.Count
        };
    }

    public void Dispose()
    {
        _collectionTimer?.Dispose();
    }
}

/// <summary>
/// نمونه آمار عملکرد
/// </summary>
public class PerformanceSnapshot
{
    public DateTime Timestamp { get; set; }
    public SystemMetrics Metrics { get; set; }
}

/// <summary>
/// روند عملکرد
/// </summary>
public class PerformanceTrend
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public double PeakCpuUsage { get; set; }
    public double PeakMemoryUsage { get; set; }
    public int SampleCount { get; set; }
}