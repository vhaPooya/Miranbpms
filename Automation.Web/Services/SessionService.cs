using Microsoft.AspNetCore.Http;
using Automation.Core.Interfaces;

namespace Automation.Web.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت Session
/// </summary>
public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CurrentSecretariatIdKey = "CurrentSecretariatId";
    private const string CurrentUserIdKey = "CurrentUserId";
    private const string CurrentPositionIdKey = "CurrentPositionId";

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentSecretariatId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return null;

        var value = session.GetString(CurrentSecretariatIdKey);
        if (string.IsNullOrEmpty(value))
            return null;

        return int.TryParse(value, out var id) ? id : null;
    }

    public void SetCurrentSecretariatId(int? secretariatId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        if (secretariatId.HasValue)
        {
            session.SetString(CurrentSecretariatIdKey, secretariatId.Value.ToString());
        }
        else
        {
            session.Remove(CurrentSecretariatIdKey);
        }
    }

    public int? GetCurrentUserId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return null;

        var value = session.GetString(CurrentUserIdKey);
        if (string.IsNullOrEmpty(value))
            return null;

        return int.TryParse(value, out var id) ? id : null;
    }

    public void SetCurrentUserId(int userId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        session.SetString(CurrentUserIdKey, userId.ToString());
    }

    public int? GetCurrentPositionId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return null;

        var value = session.GetString(CurrentPositionIdKey);
        if (string.IsNullOrEmpty(value))
            return null;

        return int.TryParse(value, out var id) ? id : null;
    }

    public void SetCurrentPositionId(int? positionId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        if (positionId.HasValue)
            session.SetString(CurrentPositionIdKey, positionId.Value.ToString());
        else
            session.Remove(CurrentPositionIdKey);
    }

    public void Clear()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        session.Clear();
    }
}



