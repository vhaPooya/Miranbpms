using System.Text.Json;
using System.Text;
using Automation.Core.Entities;

namespace Automation.Infrastructure.Services.Integration.ThirdPartyConnectors;

/// <summary>
/// اتصال به سیستم‌های ERP
/// </summary>
public class ErpConnector : IThirdPartyConnector
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public ErpConnector(HttpClient httpClient, string baseUrl, string apiKey)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _apiKey = apiKey;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// دریافت اطلاعات کارمندان
    /// </summary>
    public async Task<List<EmployeeInfo>> GetEmployeesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/employees");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var employees = JsonSerializer.Deserialize<List<EmployeeInfo>>(jsonContent);
            return employees ?? new List<EmployeeInfo>();
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve employees from ERP system", ex);
        }
    }

    /// <summary>
    /// دریافت اطلاعات بخش‌ها
    /// </summary>
    public async Task<List<DepartmentInfo>> GetDepartmentsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/departments");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var departments = JsonSerializer.Deserialize<List<DepartmentInfo>>(jsonContent);
            return departments ?? new List<DepartmentInfo>();
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve departments from ERP system", ex);
        }
    }

    /// <summary>
    /// ایجاد سفارش خرید
    /// </summary>
    public async Task<bool> CreatePurchaseOrderAsync(PurchaseOrderRequest order)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(order);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/purchase-orders", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to create purchase order in ERP system", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی وضعیت سفارش
    /// </summary>
    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        try
        {
            var updateRequest = new { Status = status };
            var jsonContent = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/orders/{orderId}/status", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to update order status in ERP system", ex);
        }
    }

    /// <summary>
    /// دریافت گزارش‌های مالی
    /// </summary>
    public async Task<FinancialReport> GetFinancialReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var queryParams = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/reports/financial{queryParams}");
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FinancialReport>(jsonContent);
        }
        catch (Exception ex)
        {
            throw new IntegrationException("Failed to retrieve financial report from ERP system", ex);
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
/// اطلاعات کارمند
/// </summary>
public class EmployeeInfo
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NationalId { get; set; }
    public string Department { get; set; }
    public string Position { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// اطلاعات بخش
/// </summary>
public class DepartmentInfo
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int? ParentDepartmentId { get; set; }
    public string Manager { get; set; }
    public int EmployeeCount { get; set; }
}

/// <summary>
/// درخواست سفارش خرید
/// </summary>
public class PurchaseOrderRequest
{
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }
    public string Notes { get; set; }
    public int RequesterId { get; set; }
}

/// <summary>
/// آیتم سفارش خرید
/// </summary>
public class PurchaseOrderItem
{
    public string ItemCode { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string UnitOfMeasure { get; set; }
}

/// <summary>
/// گزارش مالی
/// </summary>
public class FinancialReport
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
    public decimal Assets { get; set; }
    public decimal Liabilities { get; set; }
    public List<FinancialReportItem> Items { get; set; } = new List<FinancialReportItem>();
}

/// <summary>
/// آیتم گزارش مالی
/// </summary>
public class FinancialReportItem
{
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
}