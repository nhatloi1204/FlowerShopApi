using System.Security.Claims;

namespace FlowerShop.API.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("id");
        if (claim == null) 
            throw new UnauthorizedAccessException("Cannot find user identifier in token.");
            
        return long.Parse(claim.Value);
    }
}