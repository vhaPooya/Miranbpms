using System.ServiceModel;
using System.Text.Json;
using System.Xml.Serialization;
using System.Xml;

namespace Automation.Infrastructure.Services.Integration.WebServices;

/// <summary>
/// کلاینت سرویس‌های وب SOAP
/// </summary>
public class WebServiceClient
{
    private readonly ILogger<WebServiceClient> _logger;

    public WebServiceClient(ILogger<WebServiceClient> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// فراخوانی متد SOAP
    /// </summary>
    public async Task<TResponse> CallSoapServiceAsync<TRequest, TResponse>(
        string serviceUrl, 
        string action, 
        TRequest request, 
        Dictionary<string, string> headers = null)
    {
        try
        {
            using var client = new HttpClient();
            
            // تنظیم هدرها
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            
            client.DefaultRequestHeaders.Add("SOAPAction", action);
            client.DefaultRequestHeaders.Add("Content-Type", "text/xml; charset=utf-8");

            // تبدیل درخواست به XML
            var soapEnvelope = CreateSoapEnvelope(request, action);
            var content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");

            var response = await client.PostAsync(serviceUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            
            // پردازش پاسخ SOAP
            var responseObject = ParseSoapResponse<TResponse>(responseContent);
            return responseObject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to call SOAP service: {serviceUrl}, Action: {action}");
            throw new WebServiceException($"Failed to call SOAP service: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ایجاد envelope SOAP
    /// </summary>
    private string CreateSoapEnvelope<TRequest>(TRequest request, string action)
    {
        var xmlSerializer = new XmlSerializer(typeof(TRequest));
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });
        
        xmlSerializer.Serialize(xmlWriter, request);
        var requestBody = stringWriter.ToString();

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <{action} xmlns=""http://tempuri.org/"">
            {requestBody}
        </{action}>
    </soap:Body>
</soap:Envelope>";
    }

    /// <summary>
    /// پردازش پاسخ SOAP
    /// </summary>
    private T ParseSoapResponse<T>(string soapResponse)
    {
        // اینجا باید پاسخ SOAP را تجزیه کنیم
        // برای سادگی، یک نمونه پاسخ برمی‌گردانیم
        return Activator.CreateInstance<T>();
    }

    /// <summary>
    /// فراخوانی REST API
    /// </summary>
    public async Task<RestApiResponse<T>> CallRestApiAsync<T>(
        string baseUrl,
        string endpoint,
        HttpMethod method,
        object requestData = null,
        Dictionary<string, string> headers = null,
        Dictionary<string, string> queryParams = null)
    {
        try
        {
            using var client = new HttpClient();
            
            // تنظیم هدرها
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            // ساخت URL با پارامترهای کوئری
            var fullUrl = BuildUrlWithQueryParams($"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}", queryParams);

            HttpRequestMessage request = new HttpRequestMessage(method, fullUrl);
            
            // تنظیم بدن درخواست برای متدهایی که نیاز دارند
            if (requestData != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                var jsonContent = JsonSerializer.Serialize(requestData);
                request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(responseContent);
                
                return new RestApiResponse<T>
                {
                    Data = data,
                    IsSuccess = true,
                    StatusCode = response.StatusCode,
                    ResponseTimeMs = 0 // باید زمان پاسخ را اندازه‌گیری کنیم
                };
            }
            else
            {
                return new RestApiResponse<T>
                {
                    Data = default(T),
                    IsSuccess = false,
                    StatusCode = response.StatusCode,
                    ErrorMessage = responseContent
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to call REST API: {baseUrl}/{endpoint}");
            throw new WebServiceException($"Failed to call REST API: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ساخت URL با پارامترهای کوئری
    /// </summary>
    private string BuildUrlWithQueryParams(string baseUrl, Dictionary<string, string> queryParams)
    {
        if (queryParams == null || !queryParams.Any())
            return baseUrl;

        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{baseUrl}?{queryString}";
    }

    /// <summary>
    /// دریافت WSDL سرویس
    /// </summary>
    public async Task<string> GetWsdlAsync(string serviceUrl)
    {
        try
        {
            using var client = new HttpClient();
            var wsdlUrl = serviceUrl.EndsWith("?wsdl") ? serviceUrl : $"{serviceUrl}?wsdl";
            
            var response = await client.GetAsync(wsdlUrl);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve WSDL from: {serviceUrl}");
            throw new WebServiceException($"Failed to retrieve WSDL: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// تست اتصال به سرویس
    /// </summary>
    public async Task<bool> TestConnectionAsync(string serviceUrl)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(serviceUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// پاسخ REST API
/// </summary>
public class RestApiResponse<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public System.Net.HttpStatusCode StatusCode { get; set; }
    public string ErrorMessage { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// استثناهای سرویس‌های وب
/// </summary>
public class WebServiceException : Exception
{
    public WebServiceException(string message) : base(message) { }
    
    public WebServiceException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// کلاینت سرویس‌های وب خاص
/// </summary>
public class SpecificWebServiceClients
{
    private readonly WebServiceClient _webServiceClient;

    public SpecificWebServiceClients(WebServiceClient webServiceClient)
    {
        _webServiceClient = webServiceClient;
    }

    /// <summary>
    /// کلاینت سرویس ارسال پیامک
    /// </summary>
    public class SmsServiceClient
    {
        private readonly WebServiceClient _client;
        private readonly string _serviceUrl;
        private readonly string _authToken;

        public SmsServiceClient(WebServiceClient client, string serviceUrl, string authToken)
        {
            _client = client;
            _serviceUrl = serviceUrl;
            _authToken = authToken;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            var request = new SmsSendRequest
            {
                PhoneNumber = phoneNumber,
                Message = message,
                AuthToken = _authToken
            };

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_authToken}"
            };

            try
            {
                var response = await _client.CallRestApiAsync<SmsSendResponse>(
                    _serviceUrl, 
                    "/api/sms/send", 
                    HttpMethod.Post, 
                    request, 
                    headers);
                
                return response.IsSuccess && response.Data?.IsSent == true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// کلاینت سرویس پرداخت‌های آنلاین
    /// </summary>
    public class PaymentServiceClient
    {
        private readonly WebServiceClient _client;
        private readonly string _serviceUrl;
        private readonly string _apiKey;

        public PaymentServiceClient(WebServiceClient client, string serviceUrl, string apiKey)
        {
            _client = client;
            _serviceUrl = serviceUrl;
            _apiKey = apiKey;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_apiKey}",
                ["Content-Type"] = "application/json"
            };

            try
            {
                var response = await _client.CallRestApiAsync<PaymentResult>(
                    _serviceUrl,
                    "/api/payments/process",
                    HttpMethod.Post,
                    request,
                    headers);

                return response.Data ?? new PaymentResult { IsSuccess = false, ErrorMessage = "No response from payment service" };
            }
            catch (Exception ex)
            {
                return new PaymentResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<PaymentStatus> GetPaymentStatusAsync(string transactionId)
        {
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_apiKey}"
            };

            try
            {
                var response = await _client.CallRestApiAsync<PaymentStatus>(
                    _serviceUrl,
                    $"/api/payments/status/{transactionId}",
                    HttpMethod.Get,
                    null,
                    headers);

                return response.Data ?? new PaymentStatus { Status = "UNKNOWN" };
            }
            catch (Exception ex)
            {
                return new PaymentStatus { Status = "ERROR", ErrorMessage = ex.Message };
            }
        }
    }
}

/// <summary>
/// درخواست ارسال پیامک
/// </summary>
public class SmsSendRequest
{
    public string PhoneNumber { get; set; }
    public string Message { get; set; }
    public string AuthToken { get; set; }
}

/// <summary>
/// پاسخ ارسال پیامک
/// </summary>
public class SmsSendResponse
{
    public bool IsSent { get; set; }
    public string MessageId { get; set; }
    public string Status { get; set; }
}

/// <summary>
/// درخواست پرداخت
/// </summary>
public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IRR";
    public string Description { get; set; }
    public string CallbackUrl { get; set; }
    public string MobileNumber { get; set; }
    public string Email { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// نتیجه پرداخت
/// </summary>
public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string TransactionId { get; set; }
    public string Authority { get; set; }
    public string GatewayUrl { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// وضعیت پرداخت
/// </summary>
public class PaymentStatus
{
    public string Status { get; set; } // PENDING, SUCCESS, FAILED, CANCELLED
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReferenceNumber { get; set; }
    public string ErrorMessage { get; set; }
}