namespace RealtimeMeetingAPI.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IRoomRepository RoomRepository { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}
