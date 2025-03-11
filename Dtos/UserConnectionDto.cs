namespace RealtimeMeetingAPI.Dtos
{
    public class UserConnectionDto
    {
        public UserConnectionDto(string userName, Guid? roomId)
        {
            UserName = userName;
            RoomId = roomId;
        }
        public string UserName { get; set; }
        public Guid? RoomId { get; set; }
    }
}
