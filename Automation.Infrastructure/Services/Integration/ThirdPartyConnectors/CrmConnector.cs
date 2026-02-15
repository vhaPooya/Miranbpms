using System.Text.Json;
using System.Text;
using Automation.Core.Entities;

namespace Automation.Infrastructure.Services.Integration.ThirdPartyConnectors;

/// <summary>
/// اتصال به سیستم‌های CRM
/// </summary>
public class CrmConnector : IThirdPartyConnector
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public CrmConnector(HttpClient httpClient, string baseUrl, string apiKey)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _apiKey = apiKey;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// دریافت مشتریان
    /// </summary>
    public async Task<List<CustomerInfo>> GetCustomersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/customers");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var customers = JsonSerializer.Deserialize<List<CustomerInfo>>(jsonContent);
            return customers ?? new List<CustomerInfo>();
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve customers from CRM system", ex);
        }
    }

    /// <summary>
    /// دریافت فرصت‌های فروش
    /// </summary>
    public async Task<List<OpportunityInfo>> GetOpportunitiesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/opportunities");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var opportunities = JsonSerializer.Deserialize<List<OpportunityInfo>>(jsonContent);
            return opportunities ?? new List<OpportunityInfo>();
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve opportunities from CRM system", ex);
        }
    }

    /// <summary>
    /// ایجاد تماس با مشتری
    /// </summary>
    public async Task<bool> CreateCustomerInteractionAsync(CustomerInteraction interaction)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(interaction);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/interactions", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to create customer interaction in CRM system", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی اطلاعات مشتری
    /// </summary>
    public async Task<bool> UpdateCustomerAsync(int customerId, CustomerUpdateRequest updateData)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(updateData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/customers/{customerId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to update customer in CRM system", ex);
        }
    }

    /// <summary>
    /// ایجاد فرصت فروش
    /// </summary>
    public async Task<OpportunityInfo> CreateOpportunityAsync(OpportunityCreateRequest opportunity)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(opportunity);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/opportunities", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OpportunityInfo>(responseContent);
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to create opportunity in CRM system", ex);
        }
    }

    /// <summary>
    /// دریافت فعالیت‌های مشتری
    /// </summary>
    public async Task<List<CustomerActivity>> GetCustomerActivitiesAsync(int customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/customers/{customerId}/activities");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var activities = JsonSerializer.Deserialize<List<CustomerActivity>>(jsonContent);
            return activities ?? new List<CustomerActivity>();
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve customer activities from CRM system", ex);
        }
    }

    /// <summary>
    /// ارسال ایمیل به مشتری
    /// </summary>
    public async Task<bool> SendEmailToCustomerAsync(int customerId, EmailRequest emailRequest)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(emailRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/customers/{customerId}/email", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to send email to customer via CRM system", ex);
        }
    }

    /// <summary>
    /// دریافت گزارش‌های فروش
    /// </summary>
    public async Task<SalesReport> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var queryParams = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/reports/sales{queryParams}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalesReport>(jsonContent);
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve sales report from CRM system", ex);
        }
    }

    /// <summary>
    /// تست اتصال
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/status");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// اطلاعات مشتری
/// </summary>
public class CustomerInfo
{
    public int Id { get; set; }
    public string CustomerNumber { get; set; }
    public string CompanyName { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string CustomerType { get; set; }
    public decimal AnnualRevenue { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CustomerSince { get; set; }
    public string Status { get; set; }
}

/// <summary>
/// اطلاعات فرصت فروش
/// </summary>
public class OpportunityInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal EstimatedValue { get; set; }
    public string Stage { get; set; }
    public DateTime CloseDate { get; set; }
    public string Probability { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// تعامل با مشتری
/// </summary>
public class CustomerInteraction
{
    public int CustomerId { get; set; }
    public string InteractionType { get; set; } // CALL, EMAIL, MEETING, etc.
    public string Subject { get; set; }
    public string Description { get; set; }
    public DateTime InteractionDate { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int DurationMinutes { get; set; }
    public string Outcome { get; set; }
}

/// <summary>
/// درخواست به‌روزرسانی مشتری
/// </summary>
public class CustomerUpdateRequest
{
    public string CompanyName { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string CustomerType { get; set; }
    public decimal AnnualRevenue { get; set; }
    public int EmployeeCount { get; set; }
    public string Status { get; set; }
}

/// <summary>
/// درخواست ایجاد فرصت
/// </summary>
public class OpportunityCreateRequest
{
    public string Name { get; set; }
    public int CustomerId { get; set; }
    public decimal EstimatedValue { get; set; }
    public string Stage { get; set; }
    public DateTime CloseDate { get; set; }
    public string Probability { get; set; }
    public int OwnerId { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// فعالیت مشتری
/// </summary>
public class CustomerActivity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string ActivityType { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Status { get; set; }
}

/// <summary>
/// درخواست ایمیل
/// </summary>
public class EmailRequest
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<string> Attachments { get; set; } = new List<string>();
    public string TemplateId { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// گزارش فروش
/// </summary>
public class SalesReport
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalOpportunities { get; set; }
    public int ClosedWonOpportunities { get; set; }
    public decimal WinRate { get; set; }
    public decimal AverageDealSize { get; set; }
    public List<SalesReportItem> Items { get; set; } = new List<SalesReportItem>();
}

/// <summary>
/// آیتم گزارش فروش
/// </summary>
public class SalesReportItem
{
    public string SalesRep { get; set; }
    public decimal SalesAmount { get; set; }
    public int OpportunitiesCount { get; set; }
    public int ClosedWonCount { get; set; }
    public decimal ConversionRate { get; set; }
}