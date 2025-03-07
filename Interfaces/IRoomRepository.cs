using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Helpers;

namespace RealtimeMeetingAPI.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> GetRoomById(int roomId);
        Task<Room> GetRoomForConnection(string connectionId);
        void RemoveConnection(Connection connection);
        void AddRoom(Room room);
        Task<Room> DeleteRoom(int id);
        Task<Room> EditRoom(int id, string newName);
        Task DeleteAllRoom();
        Task<PagedList<RoomDto>> GetAllRoomAsync(RoomParams roomParams);
        Task<RoomDto> GetRoomDtoById(int roomId);
        Task UpdateCountMember(int roomId, int count);
    }
}
