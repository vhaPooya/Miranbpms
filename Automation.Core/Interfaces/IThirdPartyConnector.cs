using System;
using System.Threading.Tasks;

namespace Automation.Core.Interfaces;

/// <summary>
/// رابط اتصال به سیستم‌های خارجی
/// </summary>
public interface IThirdPartyConnector : IDisposable
{
    /// <summary>
    /// تست اتصال
    /// </summary>
    Task<bool> TestConnectionAsync();
}

/// <summary>
/// استثناهای اتصال به سیستم‌های خارجی
/// </summary>
public class IntegrationException : Exception
{
    public IntegrationException(string message) : base(message) { }
    
    public IntegrationException(string message, Exception innerException) 
        : base(message, innerException) { }
}