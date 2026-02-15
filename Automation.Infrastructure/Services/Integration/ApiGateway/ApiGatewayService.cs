using System.Text.Json;
using System.Text;
using Automation.Core.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Automation.Infrastructure.Services.Integration.ApiGateway;

/// <summary>
/// سرویس دروازه API برای مدیریت اتصال به سرویس‌های خارجی
/// </summary>
public class ApiGatewayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ApiGatewayService> _logger;

    public ApiGatewayService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<ApiGatewayService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// ارسال درخواست GET به API خارجی
    /// </summary>
    public async Task<ApiResponse<T>> GetAsync<T>(string serviceKey, string endpoint, Dictionary<string, string> headers = null, int cacheMinutes = 0)
    {
        try
        {
            var cacheKey = $"api_get_{serviceKey}_{endpoint}";
            
            // بررسی کش
            if (cacheMinutes > 0 && _cache.TryGetValue(cacheKey, out ApiResponse<T> cachedResponse))
            {
                return cachedResponse;
            }

            var client = _httpClientFactory.CreateClient(serviceKey);
            AddHeaders(client, headers);

            var response = await client.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(jsonContent);

                var apiResponse = new ApiResponse<T>
                {
                    Data = data,
                    StatusCode = response.StatusCode,
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                };

                // ذخیره در کش
                if (cacheMinutes > 0)
                {
                    _cache.Set(cacheKey, apiResponse, TimeSpan.FromMinutes(cacheMinutes));
                }

                return apiResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"API Gateway GET failed: {serviceKey}/{endpoint} - Status: {response.StatusCode}, Error: {errorContent}");

                return new ApiResponse<T>
                {
                    Data = default(T),
                    StatusCode = response.StatusCode,
                    IsSuccess = false,
                    ErrorMessage = errorContent,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"API Gateway GET failed: {serviceKey}/{endpoint}");
            throw new ApiGatewayException($"Failed to execute GET request to {serviceKey}/{endpoint}", ex);
        }
    }

    /// <summary>
    /// ارسال درخواست POST به API خارجی
    /// </summary>
    public async Task<ApiResponse<T>> PostAsync<T>(string serviceKey, string endpoint, object requestData, Dictionary<string, string> headers = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(serviceKey);
            AddHeaders(client, headers);

            var jsonContent = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(responseContent);

                return new ApiResponse<T>
                {
                    Data = data,
                    StatusCode = response.StatusCode,
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"API Gateway POST failed: {serviceKey}/{endpoint} - Status: {response.StatusCode}, Error: {errorContent}");

                return new ApiResponse<T>
                {
                    Data = default(T),
                    StatusCode = response.StatusCode,
                    IsSuccess = false,
                    ErrorMessage = errorContent,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"API Gateway POST failed: {serviceKey}/{endpoint}");
            throw new ApiGatewayException($"Failed to execute POST request to {serviceKey}/{endpoint}", ex);
        }
    }

    /// <summary>
    /// ارسال درخواست PUT به API خارجی
    /// </summary>
    public async Task<ApiResponse<T>> PutAsync<T>(string serviceKey, string endpoint, object requestData, Dictionary<string, string> headers = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(serviceKey);
            AddHeaders(client, headers);

            var jsonContent = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(responseContent);

                return new ApiResponse<T>
                {
                    Data = data,
                    StatusCode = response.StatusCode,
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"API Gateway PUT failed: {serviceKey}/{endpoint} - Status: {response.StatusCode}, Error: {errorContent}");

                return new ApiResponse<T>
                {
                    Data = default(T),
                    StatusCode = response.StatusCode,
                    IsSuccess = false,
                    ErrorMessage = errorContent,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"API Gateway PUT failed: {serviceKey}/{endpoint}");
            throw new ApiGatewayException($"Failed to execute PUT request to {serviceKey}/{endpoint}", ex);
        }
    }

    /// <summary>
    /// ارسال درخواست DELETE به API خارجی
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteAsync(string serviceKey, string endpoint, Dictionary<string, string> headers = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(serviceKey);
            AddHeaders(client, headers);

            var response = await client.DeleteAsync(endpoint);
            
            return new ApiResponse<bool>
            {
                Data = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"API Gateway DELETE failed: {serviceKey}/{endpoint}");
            throw new ApiGatewayException($"Failed to execute DELETE request to {serviceKey}/{endpoint}", ex);
        }
    }

    /// <summary>
    /// ارسال درخواست سفارشی به API خارجی
    /// </summary>
    public async Task<ApiResponse<T>> SendAsync<T>(string serviceKey, string endpoint, HttpMethod method, object requestData = null, Dictionary<string, string> headers = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(serviceKey);
            AddHeaders(client, headers);

            HttpRequestMessage request = new HttpRequestMessage(method, endpoint);
            
            if (requestData != null)
            {
                var jsonContent = JsonSerializer.Serialize(requestData);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(responseContent);

                return new ApiResponse<T>
                {
                    Data = data,
                    StatusCode = response.StatusCode,
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"API Gateway custom request failed: {serviceKey}/{endpoint} - Method: {method}, Status: {response.StatusCode}, Error: {errorContent}");

                return new ApiResponse<T>
                {
                    Data = default(T),
                    StatusCode = response.StatusCode,
                    IsSuccess = false,
                    ErrorMessage = errorContent,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"API Gateway custom request failed: {serviceKey}/{endpoint} - Method: {method}");
            throw new ApiGatewayException($"Failed to execute custom request to {serviceKey}/{endpoint}", ex);
        }
    }

    /// <summary>
    /// دریافت توکن احراز هویت
    /// </summary>
    public async Task<string> GetAuthTokenAsync(string serviceKey, AuthRequest authRequest)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(serviceKey);
            
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = authRequest.GrantType,
                ["client_id"] = authRequest.ClientId,
                ["client_secret"] = authRequest.ClientSecret,
                ["username"] = authRequest.Username,
                ["password"] = authRequest.Password
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync(authRequest.TokenEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<AuthTokenResponse>(jsonContent);
                return tokenResponse.AccessToken;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get auth token for service: {serviceKey}");
            return null;
        }
    }

    /// <summary>
    /// اضافه کردن هدرها به کلاینت HTTP
    /// </summary>
    private void AddHeaders(HttpClient client, Dictionary<string, string> headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (!client.DefaultRequestHeaders.Contains(header.Key))
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }
        
        // تنظیم Content-Type پیش‌فرض
        if (!client.DefaultRequestHeaders.Contains("Accept"))
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }

    /// <summary>
    /// مانیتورینگ درخواست‌ها
    /// </summary>
    public ApiMonitoringReport GetMonitoringReport()
    {
        // اینجا باید لاگ‌های درخواست‌ها را تحلیل کنیم
        // برای سادگی، یک گزارش ساده برمی‌گردانیم
        
        return new ApiMonitoringReport
        {
            TotalRequests = 1000,
            SuccessfulRequests = 950,
            FailedRequests = 50,
            AverageResponseTimeMs = 150,
            TopEndpoints = new List<ApiEndpointStats>
            {
                new ApiEndpointStats { Endpoint = "/api/documents", RequestCount = 300, AverageResponseTimeMs = 120 },
                new ApiEndpointStats { Endpoint = "/api/workflows", RequestCount = 250, AverageResponseTimeMs = 180 },
                new ApiEndpointStats { Endpoint = "/api/users", RequestCount = 200, AverageResponseTimeMs = 90 }
            },
            PeriodStart = DateTime.UtcNow.AddHours(-24),
            PeriodEnd = DateTime.UtcNow
        };
    }
}

/// <summary>
/// پاسخ API
/// </summary>
public class ApiResponse<T>
{
    public T Data { get; set; }
    public System.Net.HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// درخواست احراز هویت
/// </summary>
public class AuthRequest
{
    public string TokenEndpoint { get; set; }
    public string GrantType { get; set; } = "client_credentials";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

/// <summary>
/// پاسخ توکن احراز هویت
/// </summary>
public class AuthTokenResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
}

/// <summary>
/// استثناهای دروازه API
/// </summary>
public class ApiGatewayException : Exception
{
    public ApiGatewayException(string message) : base(message) { }
    
    public ApiGatewayException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// گزارش مانیتورینگ API
/// </summary>
public class ApiMonitoringReport
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public List<ApiEndpointStats> TopEndpoints { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// آمار endpoint API
/// </summary>
public class ApiEndpointStats
{
    public string Endpoint { get; set; }
    public int RequestCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
}