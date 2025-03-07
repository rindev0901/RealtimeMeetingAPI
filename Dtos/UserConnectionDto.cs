namespace RealtimeMeetingAPI.Dtos
{
    public class UserConnectionDto
    {
        public UserConnectionDto(string userName, int roomId)
        {
            UserName = userName;
            RoomId = roomId;
        }
        public string UserName { get; set; }
        public int RoomId { get; set; }
    }
}
