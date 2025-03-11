using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RealtimeMeetingAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("JwtSettings:Secret").Value!));
        }

        public async Task<string> CreateTokenAsync(AppUser appUser)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                new Claim(ClaimTypes.Name, appUser.UserName),
                new Claim(ClaimTypes.GivenName, appUser.FullName),
                new Claim("picture", appUser.PhotoUrl),
                new Claim("last_active", appUser.LastActive.ToString()),
            };

            var roles = await _userManager.GetRolesAsync(appUser);

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
