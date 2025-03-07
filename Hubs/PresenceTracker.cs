using RealtimeMeetingAPI.Dtos;

namespace RealtimeMeetingAPI.Hubs
{
    public class PresenceTracker
    {
        private static readonly Dictionary<UserConnectionDto, List<string>> OnlineUsers = new Dictionary<UserConnectionDto, List<string>>();

        public Task<bool> UserConnected(UserConnectionDto user, string connectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                var temp = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if (temp.Key == null) //Not has connection
                {
                    OnlineUsers.Add(user, new List<string> { connectionId });
                    isOnline = true;
                }
                else if (OnlineUsers.ContainsKey(temp.Key)) //Has many connection same user
                {
                    OnlineUsers[temp.Key].Add(connectionId);
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(UserConnectionDto user, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                var temp = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if (temp.Key == null)
                    return Task.FromResult(isOffline);

                OnlineUsers[temp.Key].Remove(connectionId);
                if (OnlineUsers[temp.Key].Count == 0)
                {
                    OnlineUsers.Remove(temp.Key);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<List<UserConnectionDto>> GetOnlineUsers(int roomId)
        {
            List<UserConnectionDto> onlineUsers = new();
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.Where(u => u.Key.RoomId == roomId).Select(k => k.Key).ToList();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(UserConnectionDto user)
        {
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {
                var temp = OnlineUsers.FirstOrDefault(x => x.Key.UserName == user.UserName && x.Key.RoomId == user.RoomId);
                if (temp.Key != null)
                {
                    connectionIds = OnlineUsers.GetValueOrDefault(temp.Key);
                }
            }
            return Task.FromResult(connectionIds);
        }

        public Task<List<string>> GetConnectionsForUsername(string username)
        {
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {
                // 1 user co nhieu lan dang nhap
                var listTemp = OnlineUsers.Where(x => x.Key.UserName == username).ToList();
                if (listTemp.Count > 0)
                {
                    foreach (var user in listTemp)
                    {
                        connectionIds.AddRange(user.Value);
                    }
                }
            }
            return Task.FromResult(connectionIds);
        }
        public async Task SendPresenceForOnlineUsers()
        {
        }
    }
}
