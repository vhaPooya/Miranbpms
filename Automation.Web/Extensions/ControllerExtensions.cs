using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.Extensions;

/// <summary>
/// Extension methods for Controller to get current user information
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Get current user ID from claims (0 if not authenticated)
    /// </summary>
    public static int GetCurrentUserId(this Controller controller)
    {
        var userIdClaim = controller.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? controller.User?.FindFirst("UserId")?.Value
            ?? controller.User?.FindFirst("sub")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return 0;
    }

    /// <summary>
    /// Get current user ID or throw exception if not authenticated
    /// </summary>
    public static int GetCurrentUserIdRequired(this Controller controller)
    {
        var userId = controller.GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User is not authenticated");
        return userId;
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public static bool IsAuthenticated(this Controller controller)
    {
        return controller.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Get current user name from claims
    /// </summary>
    public static string? GetCurrentUserName(this Controller controller)
    {
        return controller.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? controller.User?.FindFirst("UserName")?.Value
            ?? controller.User?.Identity?.Name;
    }
}
