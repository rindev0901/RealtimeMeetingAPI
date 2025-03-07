using RealtimeMeetingAPI.Entities;

namespace RealtimeMeetingAPI.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser appUser);
    }
}
