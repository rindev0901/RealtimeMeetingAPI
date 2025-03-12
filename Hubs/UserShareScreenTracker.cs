using RealtimeMeetingAPI.Dtos;

namespace RealtimeMeetingAPI.Hubs
{
    public class UserShareScreenTracker
    {
        private static readonly List<UserConnectionDto> usersShareScreen = new List<UserConnectionDto>();

        public Task<bool> UserConnectedToShareScreen(UserConnectionDto user)
        {
            bool isOnline = false;
            lock (usersShareScreen)
            {
                var temp = usersShareScreen.FirstOrDefault(x => x.UserId == user.UserId && x.RoomId == user.RoomId);

                if (temp == null) // No users have shared the screen in this room yet
                {
                    usersShareScreen.Add(user);
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnectedShareScreen(UserConnectionDto user)
        {
            bool isOffline = false;
            lock (usersShareScreen)
            {
                var temp = usersShareScreen.FirstOrDefault(x => x.UserId == user.UserId && x.RoomId == user.RoomId);
                if (temp == null)
                    return Task.FromResult(isOffline);
                else
                {
                    usersShareScreen.Remove(temp);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<UserConnectionDto> GetUserIsSharing(Guid roomId)
        {
            UserConnectionDto temp = null;
            lock (usersShareScreen)
            {
                temp = usersShareScreen.FirstOrDefault(x => x.RoomId == roomId);
            }
            return Task.FromResult(temp);
        }

        public Task<bool> DisconnectedByUser(Guid userId, Guid roomId)
        {
            bool isOffline = false;
            lock (usersShareScreen)
            {
                var temp = usersShareScreen.FirstOrDefault(x => x.UserId == userId && x.RoomId == roomId);
                if (temp != null)
                {
                    isOffline = true;
                    usersShareScreen.Remove(temp);
                }
            }
            return Task.FromResult(isOffline);
        }
    }
}
