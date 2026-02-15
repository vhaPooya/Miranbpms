namespace Automation.Core.Entities;

/// <summary>
/// Web Services (REST / SOAP)
/// </summary>
public class WebService : BaseEntity
{
    /// <summary>
    /// نام سرویس
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// URL سرویس
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع سرویس
    /// </summary>
    public string ServiceType { get; set; } = "REST"; // REST, SOAP
    
    /// <summary>
    /// متد HTTP
    /// </summary>
    public string HttpMethod { get; set; } = "POST";
    
    /// <summary>
    /// Headers (JSON)
    /// </summary>
    public string? Headers { get; set; }
    
    /// <summary>
    /// Authentication Type
    /// </summary>
    public string? AuthType { get; set; } // None, Basic, Bearer, APIKey
    
    /// <summary>
    /// API Key یا Token
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}



