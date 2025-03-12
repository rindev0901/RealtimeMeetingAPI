namespace RealtimeMeetingAPI.Dtos
{
    public class UserConnectionDto
    {
        public UserConnectionDto(Guid userId, Guid? roomId)
        {
            UserId = userId;
            RoomId = roomId;
        }
        public Guid UserId { get; set; }
        public Guid? RoomId { get; set; }
    }
}
