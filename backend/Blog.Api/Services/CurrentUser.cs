using System.Security.Claims;

namespace Blog.Api.Services;

public static class CurrentUser
{
    public static int? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (int.TryParse(sub, out var id)) return id;

        // JWT Registered claim "sub"
        sub = user.FindFirstValue("sub");
        if (int.TryParse(sub, out id)) return id;

        return null;
    }
}
