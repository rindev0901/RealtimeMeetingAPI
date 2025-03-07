using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RealtimeMeetingAPI.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            var userName = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;

            if (!string.IsNullOrEmpty(userName))
            {
                return userName;
            }
            throw new Exception("Invalid or missing UniqueName claim");
        }

        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var claimValue = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value;

            if (Guid.TryParse(claimValue, out var userId))
            {
                return userId;
            }
            throw new Exception("Invalid or missing NameId claim");
        }
    }
}
