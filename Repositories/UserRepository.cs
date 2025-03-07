using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RealtimeMeetingAPI.Data;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Helpers;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public UserRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<MemberDto?> GetMemberAsync(string username)
        {
            //Instead of retrieving the full AppUser entity and then mapping it in-memory,
            //ProjectTo<MemberDto>() generates an optimized SQL query that selects only the necessary fields defined in MemberDto.
            return await _context.Users.Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDto>> GetUsersOnlineAsync(List<UserConnectionDto> userOnlines)
        {
            var listUserOnline = new List<MemberDto>();
            foreach (var u in userOnlines)
            {
                var user = await _context.Users.Where(x => x.UserName == u.UserName)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

                listUserOnline.Add(user);
            }
            //return await Task.Run(() => listUserOnline.ToList());
            return await Task.FromResult(listUserOnline.ToList());
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername).OrderByDescending(u => u.LastActive);

            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<IEnumerable<MemberDto>> SearchMemberAsync(string displayname)
        {
            return await _context.Users.Where(u => u.FullName.ToLower().Contains(displayname.ToLower()))
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<AppUser?> UpdateLocked(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
            if (user != null)
            {
                user.Locked = !user.Locked;
            }
            return user;
        }
    }
}
