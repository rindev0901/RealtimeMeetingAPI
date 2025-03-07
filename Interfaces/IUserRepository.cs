using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Helpers;

namespace RealtimeMeetingAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetUserByIdAsync(Guid id);
        Task<AppUser?> GetUserByUsernameAsync(string username);
        Task<MemberDto?> GetMemberAsync(string username);
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
        Task<IEnumerable<MemberDto>> SearchMemberAsync(string displayname);
        Task<IEnumerable<MemberDto>> GetUsersOnlineAsync(List<UserConnectionDto> userOnlines);
        Task<AppUser?> UpdateLocked(string username);
    }
}
