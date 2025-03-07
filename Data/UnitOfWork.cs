using AutoMapper;
using RealtimeMeetingAPI.Interfaces;
using RealtimeMeetingAPI.Repositories;

namespace RealtimeMeetingAPI.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public UnitOfWork(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        public IRoomRepository RoomRepository => new RoomRepository(_context, _mapper);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
