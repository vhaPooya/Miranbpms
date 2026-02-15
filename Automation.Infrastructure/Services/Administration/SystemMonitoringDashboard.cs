using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Automation.Infrastructure.Services.Administration;

/// <summary>
/// داشبورد نظارت بر سیستم برای مدیریت و مانیتورینگ
/// </summary>
public class SystemMonitoringDashboard
{
    private readonly ILogger<SystemMonitoringDashboard> _logger;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly Dictionary<string, PerformanceCounter> _customCounters;

    public SystemMonitoringDashboard(ILogger<SystemMonitoringDashboard> logger)
    {
        _logger = logger;
        _customCounters = new Dictionary<string, PerformanceCounter>();

        try
        {
            // تنظیم Performance Counters در ویندوز
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes", string.Empty);
                
                // اضافه کردن شمارنده‌های سفارشی
                AddCustomCounter("disk_io", "PhysicalDisk", "Disk Transfers/sec", "_Total");
                AddCustomCounter("network_in", "Network Interface", "Bytes Received/sec", "Intel[R] Ethernet Connection");
                AddCustomCounter("network_out", "Network Interface", "Bytes Sent/sec", "Intel[R] Ethernet Connection");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize performance counters");
        }
    }

    /// <summary>
    /// اضافه کردن شمارنده سفارشی
    /// </summary>
    private void AddCustomCounter(string name, string category, string counter, string instance = "")
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _customCounters[name] = new PerformanceCounter(category, counter, instance);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to add custom counter: {name}");
        }
    }

    /// <summary>
    /// دریافت آمار فعلی سیستم
    /// </summary>
    public async Task<SystemMetrics> GetCurrentMetricsAsync()
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
                metrics.TotalMemoryMB = GetTotalSystemMemory();
                metrics.MemoryUsagePercentage = ((metrics.TotalMemoryMB - metrics.AvailableMemoryMB) / metrics.TotalMemoryMB) * 100;
            }

            // Custom Counters
            foreach (var counter in _customCounters)
            {
                try
                {
                    metrics.CustomMetrics[counter.Key] = await Task.Run(() => counter.Value.NextValue());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to read counter: {counter.Key}");
                }
            }

            // Application Metrics
            metrics.ApplicationMemoryUsageMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            metrics.ThreadCount = Process.GetCurrentProcess().Threads.Count;
            metrics.HandleCount = Process.GetCurrentProcess().HandleCount;
            
            // Garbage Collection Info
            metrics.GcCollectionsGen0 = GC.CollectionCount(0);
            metrics.GcCollectionsGen1 = GC.CollectionCount(1);
            metrics.GcCollectionsGen2 = GC.CollectionCount(2);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system metrics");
            return metrics;
        }
    }

    /// <summary>
    /// دریافت حافظه کل سیستم
    /// </summary>
    private double GetTotalSystemMemory()
    {
        try
        {
            // در ویندوز
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return (double)(Environment.WorkingSet / (1024 * 1024));
            }
            // در لینوکس
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var psi = new ProcessStartInfo("cat", "/proc/meminfo")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                
                using var process = Process.Start(psi);
                using var reader = process.StandardOutput;
                var output = reader.ReadToEnd();
                
                // پیدا کردن MemTotal
                var memTotalLine = output.Split('\n')
                    .FirstOrDefault(line => line.StartsWith("MemTotal:"));
                
                if (memTotalLine != null)
                {
                    var parts = memTotalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1 && long.TryParse(parts[1], out var memTotalKB))
                    {
                        return memTotalKB / 1024.0; // تبدیل از KB به MB
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get total system memory");
        }
        
        return 8192; // مقدار پیش‌فرض 8GB
    }

    /// <summary>
    /// دریافت آمار پایگاه داده
    /// </summary>
    public async Task<DatabaseMetrics> GetDatabaseMetricsAsync()
    {
        // اینجا باید آمار واقعی دیتابیس را دریافت کنیم
        return new DatabaseMetrics
        {
            ActiveConnections = 15,
            ConnectionPoolSize = 100,
            QueriesPerSecond = 45.5,
            AverageQueryTimeMs = 125.7,
            SlowQueries = 3,
            Deadlocks = 0,
            LockWaits = 2,
            DatabaseSizeMB = 1250.5,
            BufferCacheHitRatio = 98.5,
            TransactionsPerSecond = 120.3
        };
    }

    /// <summary>
    /// دریافت آمار کش
    /// </summary>
    public async Task<CacheMetrics> GetCacheMetricsAsync()
    {
        // اینجا باید آمار واقعی کش را دریافت کنیم
        return new CacheMetrics
        {
            HitRatePercentage = 85.5,
            MissRatePercentage = 14.5,
            CurrentEntries = 12500,
            MemoryUsageMB = 45.2,
            Evictions = 120,
            Errors = 0,
            KeyspaceHits = 45000,
            KeyspaceMisses = 8000
        };
    }

    /// <summary>
    /// دریافت آمار HTTP
    /// </summary>
    public async Task<HttpMetrics> GetHttpMetricsAsync()
    {
        // اینجا باید آمار واقعی HTTP را دریافت کنیم
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
            },
            ActiveConnections = 250,
            TotalRequests = 50000,
            FailedRequests = 400
        };
    }

    /// <summary>
    /// دریافت گزارش کامل سیستم
    /// </summary>
    public async Task<SystemReport> GenerateFullSystemReportAsync()
    {
        var report = new SystemReport
        {
            GeneratedAt = DateTime.UtcNow,
            SystemMetrics = await GetCurrentMetricsAsync(),
            DatabaseMetrics = await GetDatabaseMetricsAsync(),
            CacheMetrics = await GetCacheMetricsAsync(),
            HttpMetrics = await GetHttpMetricsAsync()
        };

        // تعیین وضعیت کلی
        report.HealthStatus = DetermineSystemHealth(report);
        report.Recommendations = GenerateRecommendations(report);

        return report;
    }

    /// <summary>
    /// تعیین سلامت سیستم
    /// </summary>
    private HealthStatus DetermineSystemHealth(SystemReport report)
    {
        // بررسی شرایط بحرانی
        if (report.SystemMetrics.CpuUsagePercentage > 90)
            return HealthStatus.Critical;

        if (report.SystemMetrics.MemoryUsagePercentage > 85)
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
    /// تولید توصیه‌ها
    /// </summary>
    private List<string> GenerateRecommendations(SystemReport report)
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
    /// جمع‌آوری داده‌های تاریخی
    /// </summary>
    public async Task<HistoricalMetrics> GetHistoricalMetricsAsync(TimeSpan period)
    {
        // اینجا باید داده‌های تاریخی را از دیتابیس بخوانیم
        return new HistoricalMetrics
        {
            PeriodStart = DateTime.UtcNow.Subtract(period),
            PeriodEnd = DateTime.UtcNow,
            AverageCpuUsage = 45.5,
            AverageMemoryUsage = 65.2,
            PeakCpuUsage = 85.7,
            PeakMemoryUsage = 82.3,
            TotalRequests = 1250000,
            ErrorCount = 1200
        };
    }

    /// <summary>
    /// نمایش وضعیت سیستم در کنسول
    /// </summary>
    public async Task DisplayConsoleDashboardAsync()
    {
        var report = await GenerateFullSystemReportAsync();
        
        Console.Clear();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("                  SYSTEM MONITORING DASHBOARD");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();
        
        // System Metrics
        Console.WriteLine("SYSTEM METRICS:");
        Console.WriteLine($"  CPU Usage:     {report.SystemMetrics.CpuUsagePercentage:F1}%");
        Console.WriteLine($"  Memory Usage:  {report.SystemMetrics.MemoryUsagePercentage:F1}% ({report.SystemMetrics.ApplicationMemoryUsageMB:F1}MB App)");
        Console.WriteLine($"  Threads:       {report.SystemMetrics.ThreadCount}");
        Console.WriteLine();
        
        // Database Metrics
        Console.WriteLine("DATABASE METRICS:");
        Console.WriteLine($"  Connections:   {report.DatabaseMetrics.ActiveConnections}/{report.DatabaseMetrics.ConnectionPoolSize}");
        Console.WriteLine($"  QPS:           {report.DatabaseMetrics.QueriesPerSecond:F1}");
        Console.WriteLine($"  Avg Query Time: {report.DatabaseMetrics.AverageQueryTimeMs:F1}ms");
        Console.WriteLine();
        
        // HTTP Metrics
        Console.WriteLine("HTTP METRICS:");
        Console.WriteLine($"  RPS:           {report.HttpMetrics.RequestsPerSecond:F1}");
        Console.WriteLine($"  Avg Response:  {report.HttpMetrics.AverageResponseTimeMs:F1}ms");
        Console.WriteLine($"  Error Rate:    {report.HttpMetrics.ErrorRatePercentage:F1}%");
        Console.WriteLine();
        
        // Health Status
        var statusColor = report.HealthStatus switch
        {
            HealthStatus.Healthy => ConsoleColor.Green,
            HealthStatus.Warning => ConsoleColor.Yellow,
            HealthStatus.Critical => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
        
        Console.ForegroundColor = statusColor;
        Console.WriteLine($"SYSTEM STATUS: {report.HealthStatus}");
        Console.ResetColor();
        Console.WriteLine();
        
        // Recommendations
        if (report.Recommendations.Any())
        {
            Console.WriteLine("RECOMMENDATIONS:");
            foreach (var rec in report.Recommendations)
            {
                Console.WriteLine($"  • {rec}");
            }
        }
        
        Console.WriteLine();
        Console.WriteLine($"Last Updated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("Press any key to refresh...");
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
    public double ApplicationMemoryUsageMB { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public int GcCollectionsGen0 { get; set; }
    public int GcCollectionsGen1 { get; set; }
    public int GcCollectionsGen2 { get; set; }
    public Dictionary<string, float> CustomMetrics { get; set; } = new Dictionary<string, float>();
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
/// گزارش سیستم
/// </summary>
public class SystemReport
{
    public DateTime GeneratedAt { get; set; }
    public SystemMetrics SystemMetrics { get; set; }
    public DatabaseMetrics DatabaseMetrics { get; set; }
    public CacheMetrics CacheMetrics { get; set; }
    public HttpMetrics HttpMetrics { get; set; }
    public HealthStatus HealthStatus { get; set; }
    public List<string> Recommendations { get; set; } = new List<string>();
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
    public double DatabaseSizeMB { get; set; }
    public double BufferCacheHitRatio { get; set; }
    public double TransactionsPerSecond { get; set; }
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
    public long KeyspaceHits { get; set; }
    public long KeyspaceMisses { get; set; }
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
    public int ActiveConnections { get; set; }
    public long TotalRequests { get; set; }
    public long FailedRequests { get; set; }
}

/// <summary>
/// آمار تاریخی
/// </summary>
public class HistoricalMetrics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double AverageCpuUsage { get; set; }
    public double AverageMemoryUsage { get; set; }
    public double PeakCpuUsage { get; set; }
    public double PeakMemoryUsage { get; set; }
    public long TotalRequests { get; set; }
    public long ErrorCount { get; set; }
}