using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Helpers;

namespace RealtimeMeetingAPI.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> GetRoomById(Guid roomId);
        Task<Room> GetRoomForConnection(string connectionId);
        void RemoveConnection(Connection connection);
        void AddRoom(Room room);
        Task<Room> DeleteRoom(Guid id);
        Task<List<string>> GetRoomsForUser(Guid userId);
        Task<Room> EditRoom(Guid id, string newName);
        Task DeleteAllRoom();
        Task<PagedList<RoomDto>> GetAllRoomAsync(RoomParams roomParams);
        Task<RoomDto> GetRoomDtoById(Guid roomId);
        Task UpdateCountMember(Guid roomId, int count);
    }
}
