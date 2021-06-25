using System.Security.Claims;

namespace DatingApp.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetCurrentLoggedInUserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}