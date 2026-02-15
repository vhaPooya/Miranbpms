using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace Automation.Infrastructure.Services.Performance;

/// <summary>
/// سیستم توزیع بار برای مقیاس‌پذیری سیستم
/// </summary>
public class LoadBalancer
{
    private readonly ConcurrentDictionary<string, ServerInfo> _servers;
    private readonly ILogger<LoadBalancer> _logger;
    private readonly object _lockObject = new object();

    public LoadBalancer(ILogger<LoadBalancer> logger)
    {
        _servers = new ConcurrentDictionary<string, ServerInfo>();
        _logger = logger;
    }

    /// <summary>
    /// افزودن سرور به توزیع بار
    /// </summary>
    public void AddServer(string serverId, string ipAddress, int port, int weight = 1)
    {
        var serverInfo = new ServerInfo
        {
            ServerId = serverId,
            IpAddress = ipAddress,
            Port = port,
            Weight = weight,
            Status = ServerStatus.Healthy,
            LastHealthCheck = DateTime.UtcNow,
            RequestCount = 0
        };

        _servers.TryAdd(serverId, serverInfo);
        _logger.LogInformation($"Added server {serverId} to load balancer");
    }

    /// <summary>
    /// حذف سرور از توزیع بار
    /// </summary>
    public void RemoveServer(string serverId)
    {
        if (_servers.TryRemove(serverId, out var server))
        {
            _logger.LogInformation($"Removed server {serverId} from load balancer");
        }
    }

    /// <summary>
    /// دریافت سرور بعدی بر اساس الگوریتم Round Robin
    /// </summary>
    public ServerInfo GetNextServerRoundRobin()
    {
        lock (_lockObject)
        {
            var healthyServers = _servers.Values
                .Where(s => s.Status == ServerStatus.Healthy)
                .OrderBy(s => s.RequestCount)
                .ToList();

            if (!healthyServers.Any())
            {
                _logger.LogWarning("No healthy servers available");
                return null;
            }

            // انتخاب سرور با کمترین تعداد درخواست
            var selectedServer = healthyServers.First();
            selectedServer.RequestCount++;
            
            _logger.LogDebug($"Selected server {selectedServer.ServerId} using round-robin algorithm");
            return selectedServer;
        }
    }

    /// <summary>
    /// دریافت سرور بر اساس الگوریتم Weighted Round Robin
    /// </summary>
    public ServerInfo GetNextServerWeightedRoundRobin()
    {
        lock (_lockObject)
        {
            var healthyServers = _servers.Values
                .Where(s => s.Status == ServerStatus.Healthy)
                .ToList();

            if (!healthyServers.Any())
            {
                _logger.LogWarning("No healthy servers available");
                return null;
            }

            // محاسبه مجموع وزن‌ها
            var totalWeight = healthyServers.Sum(s => s.Weight);
            
            // انتخاب سرور بر اساس وزن
            var random = new Random();
            var randomValue = random.Next(totalWeight);
            
            int currentWeight = 0;
            foreach (var server in healthyServers)
            {
                currentWeight += server.Weight;
                if (randomValue < currentWeight)
                {
                    server.RequestCount++;
                    _logger.LogDebug($"Selected server {server.ServerId} using weighted round-robin algorithm");
                    return server;
                }
            }

            // اگر هیچ سروری انتخاب نشد، اولین سرور سالم را انتخاب می‌کنیم
            var fallbackServer = healthyServers.First();
            fallbackServer.RequestCount++;
            return fallbackServer;
        }
    }

    /// <summary>
    /// دریافت سرور بر اساس حداقل بار
    /// </summary>
    public ServerInfo GetNextServerLeastConnections()
    {
        lock (_lockObject)
        {
            var healthyServers = _servers.Values
                .Where(s => s.Status == ServerStatus.Healthy)
                .OrderBy(s => s.ActiveConnections)
                .ToList();

            if (!healthyServers.Any())
            {
                _logger.LogWarning("No healthy servers available");
                return null;
            }

            var selectedServer = healthyServers.First();
            selectedServer.ActiveConnections++;
            selectedServer.RequestCount++;
            
            _logger.LogDebug($"Selected server {selectedServer.ServerId} using least connections algorithm");
            return selectedServer;
        }
    }

    /// <summary>
    /// بررسی سلامت سرورها
    /// </summary>
    public async Task HealthCheckAsync()
    {
        var tasks = _servers.Values.Select(CheckServerHealthAsync).ToArray();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// بررسی سلامت یک سرور خاص
    /// </summary>
    private async Task CheckServerHealthAsync(ServerInfo server)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var healthUrl = $"http://{server.IpAddress}:{server.Port}/health";
            var response = await client.GetAsync(healthUrl);
            
            if (response.IsSuccessStatusCode)
            {
                server.Status = ServerStatus.Healthy;
                server.LastHealthCheck = DateTime.UtcNow;
                _logger.LogDebug($"Server {server.ServerId} is healthy");
            }
            else
            {
                server.Status = ServerStatus.Unhealthy;
                server.LastHealthCheck = DateTime.UtcNow;
                _logger.LogWarning($"Server {server.ServerId} is unhealthy: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            server.Status = ServerStatus.Down;
            server.LastHealthCheck = DateTime.UtcNow;
            _logger.LogError(ex, $"Server {server.ServerId} health check failed");
        }
    }

    /// <summary>
    /// دریافت آمار توزیع بار
    /// </summary>
    public LoadBalancingStats GetStats()
    {
        return new LoadBalancingStats
        {
            TotalServers = _servers.Count,
            HealthyServers = _servers.Values.Count(s => s.Status == ServerStatus.Healthy),
            UnhealthyServers = _servers.Values.Count(s => s.Status == ServerStatus.Unhealthy),
            DownServers = _servers.Values.Count(s => s.Status == ServerStatus.Down),
            TotalRequests = _servers.Values.Sum(s => s.RequestCount),
            Servers = _servers.Values.ToList()
        };
    }

    /// <summary>
    /// تنظیم تعداد اتصالات فعال برای یک سرور
    /// </summary>
    public void UpdateServerConnections(string serverId, int activeConnections)
    {
        if (_servers.TryGetValue(serverId, out var server))
        {
            server.ActiveConnections = activeConnections;
        }
    }

    /// <summary>
    /// تنظیم وضعیت سرور
    /// </summary>
    public void SetServerStatus(string serverId, ServerStatus status)
    {
        if (_servers.TryGetValue(serverId, out var server))
        {
            server.Status = status;
            server.LastHealthCheck = DateTime.UtcNow;
            _logger.LogInformation($"Server {serverId} status changed to {status}");
        }
    }
}

/// <summary>
/// اطلاعات سرور
/// </summary>
public class ServerInfo
{
    public string ServerId { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int Weight { get; set; }
    public ServerStatus Status { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public long RequestCount { get; set; }
    public int ActiveConnections { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// وضعیت سرور
/// </summary>
public enum ServerStatus
{
    Healthy,
    Unhealthy,
    Down,
    Maintenance
}

/// <summary>
/// آمار توزیع بار
/// </summary>
public class LoadBalancingStats
{
    public int TotalServers { get; set; }
    public int HealthyServers { get; set; }
    public int UnhealthyServers { get; set; }
    public int DownServers { get; set; }
    public long TotalRequests { get; set; }
    public List<ServerInfo> Servers { get; set; } = new List<ServerInfo>();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// مدیریت توزیع بار برای سرویس‌های خاص
/// </summary>
public class ServiceLoadBalancer
{
    private readonly LoadBalancer _loadBalancer;
    private readonly ILogger<ServiceLoadBalancer> _logger;

    public ServiceLoadBalancer(LoadBalancer loadBalancer, ILogger<ServiceLoadBalancer> logger)
    {
        _loadBalancer = loadBalancer;
        _logger = logger;
    }

    /// <summary>
    /// توزیع بار برای سرویس اسناد
    /// </summary>
    public ServerInfo GetDocumentServiceServer()
    {
        return _loadBalancer.GetNextServerLeastConnections();
    }

    /// <summary>
    /// توزیع بار برای سرویس فرم‌ها
    /// </summary>
    public ServerInfo GetFormServiceServer()
    {
        return _loadBalancer.GetNextServerWeightedRoundRobin();
    }

    /// <summary>
    /// توزیع بار برای سرویس گردش کار
    /// </summary>
    public ServerInfo GetWorkflowServiceServer()
    {
        return _loadBalancer.GetNextServerRoundRobin();
    }

    /// <summary>
    /// توزیع بار برای سرویس گزارش‌ها
    /// </summary>
    public ServerInfo GetReportServiceServer()
    {
        return _loadBalancer.GetNextServerLeastConnections();
    }

    /// <summary>
    /// آپدیت تعداد اتصالات فعال
    /// </summary>
    public void UpdateActiveConnections(string serverId, string serviceName, int count)
    {
        _loadBalancer.UpdateServerConnections(serverId, count);
        _logger.LogDebug($"Updated active connections for {serviceName} on server {serverId}: {count}");
    }
}

/// <summary>
/// کلاینت توزیع بار
/// </summary>
public class LoadBalancedHttpClient : IDisposable
{
    private readonly LoadBalancer _loadBalancer;
    private readonly HttpClient _httpClient;
    private readonly string _serviceName;

    public LoadBalancedHttpClient(LoadBalancer loadBalancer, string serviceName)
    {
        _loadBalancer = loadBalancer;
        _serviceName = serviceName;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// ارسال درخواست HTTP با توزیع بار
    /// </summary>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var server = _loadBalancer.GetNextServerLeastConnections();
        if (server == null)
        {
            throw new InvalidOperationException("No healthy servers available");
        }

        // تغییر URL به سرور انتخاب شده
        var originalUri = request.RequestUri;
        var newUri = new Uri($"http://{server.IpAddress}:{server.Port}{originalUri.PathAndQuery}");
        request.RequestUri = newUri;

        try
        {
            var response = await _httpClient.SendAsync(request);
            
            // آپدیت تعداد اتصالات فعال
            _loadBalancer.UpdateServerConnections(server.ServerId, server.ActiveConnections - 1);
            
            return response;
        }
        catch (Exception ex)
        {
            // علامت‌گذاری سرور به عنوان ناسالم
            _loadBalancer.SetServerStatus(server.ServerId, ServerStatus.Unhealthy);
            throw new LoadBalancingException($"Request failed for server {server.ServerId}", ex);
        }
    }

    /// <summary>
    /// دریافت داده با توزیع بار
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        var response = await SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }
        
        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// استثناهای توزیع بار
/// </summary>
public class LoadBalancingException : Exception
{
    public LoadBalancingException(string message) : base(message) { }
    
    public LoadBalancingException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// پیکربندی توزیع بار
/// </summary>
public class LoadBalancerConfiguration
{
    public List<ServerConfiguration> Servers { get; set; } = new List<ServerConfiguration>();
    public LoadBalancingAlgorithm Algorithm { get; set; } = LoadBalancingAlgorithm.LeastConnections;
    public int HealthCheckIntervalSeconds { get; set; } = 30;
    public int ServerTimeoutSeconds { get; set; } = 30;
    public bool EnableFailover { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// پیکربندی سرور
/// </summary>
public class ServerConfiguration
{
    public string ServerId { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int Weight { get; set; } = 1;
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// الگوریتم توزیع بار
/// </summary>
public enum LoadBalancingAlgorithm
{
    RoundRobin,
    WeightedRoundRobin,
    LeastConnections,
    IPHash
}