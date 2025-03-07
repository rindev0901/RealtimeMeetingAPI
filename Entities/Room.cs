using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealtimeMeetingAPI.Entities
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int CountMember { get; set; }

        public AppUser AppUser { get; set; }
        public Guid UserId { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }

    public class Connection
    {
        public Connection(string connectionId, string userName)
        {
            ConnectionId = connectionId;
            UserName = userName;
        }
        [Key]
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
    }
}
