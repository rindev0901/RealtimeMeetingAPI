using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Extensions;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        IHubContext<PresenceHub> _presenceHub;
        PresenceTracker _presenceTracker;
        IUnitOfWork _unitOfWork;
        UserShareScreenTracker _shareScreenTracker;

        public ChatHub(IUnitOfWork unitOfWork, UserShareScreenTracker shareScreenTracker, PresenceTracker presenceTracker, IHubContext<PresenceHub> presenceHub)
        {
            _unitOfWork = unitOfWork;
            _presenceTracker = presenceTracker;
            _presenceHub = presenceHub;
            _shareScreenTracker = shareScreenTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var roomId = httpContext.Request.Query["roomId"].ToString();
            var roomIdInt = Guid.Parse(roomId);
            var username = Context.User.GetUsername();

            await _presenceTracker.UserConnected(new UserConnectionDto(username, roomIdInt), Context.ConnectionId); // Danh sách quản lý các user với các connection của từng user đó

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId); // Thêm roomId bằng connection cho Hub Groups quản lý
            await AddConnectionToGroup(roomIdInt); // Và đồng bộ vào csdl

            var oneUserOnline = await _unitOfWork.UserRepository.GetMemberAsync(username); // Lấy thông tin user hiện tại đang lập connection
            await Clients.Group(roomId).SendAsync("UserOnlineInGroup", oneUserOnline);

            var currentUsers = await _presenceTracker.GetOnlineUsers(roomIdInt); // get users from dictionary by roomid

            await _unitOfWork.RoomRepository.UpdateCountMember(roomIdInt, currentUsers.Count);
            await _unitOfWork.Complete();

            var currentConnections = await _presenceTracker.GetConnectionsForUser(new UserConnectionDto(username, roomIdInt));

            await _presenceHub.Clients.AllExcept(currentConnections).SendAsync("CountMemberInGroup",
                   new { roomId = roomIdInt, countMember = currentUsers.Count });

            //share screen user vao sau cung
            var userIsSharing = await _shareScreenTracker.GetUserIsSharing(roomIdInt);
            if (userIsSharing != null)
            {
                var currentBeginConnectionsUser = await _presenceTracker.GetConnectionsForUser(userIsSharing);
                if (currentBeginConnectionsUser.Count > 0)
                    await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreenLastUser", new { usernameTo = username, isShare = true });
                await Clients.Caller.SendAsync("OnUserIsSharing", userIsSharing.UserName);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.GetUsername();
            var group = await RemoveConnectionFromGroup();
            var isOffline = await _presenceTracker.UserDisconnected(new UserConnectionDto(username, group.RoomId), Context.ConnectionId);

            await _shareScreenTracker.DisconnectedByUser(username, group.RoomId);
            if (isOffline)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.RoomId.ToString());
                var temp = await _unitOfWork.UserRepository.GetMemberAsync(username);
                await Clients.Group(group.RoomId.ToString()).SendAsync("UserOfflineInGroup", temp);

                var currentUsers = await _presenceTracker.GetOnlineUsers(group.RoomId);
                await _unitOfWork.RoomRepository.UpdateCountMember(group.RoomId, currentUsers.Count);
                await _unitOfWork.Complete();

                await _presenceHub.Clients.All.SendAsync("CountMemberInGroup",
                       new { roomId = group.RoomId, countMember = currentUsers.Count });
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var userName = Context.User.GetUsername();
            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(userName);

            var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);

            if (group != null)
            {
                var message = new MessageDto
                {
                    SenderUsername = userName,
                    SenderDisplayName = sender.FullName,
                    Content = createMessageDto.Content,
                    MessageSent = DateTime.Now
                };
                //Luu message vao db
                //code here
                //send meaasge to group
                await Clients.Group(group.RoomId.ToString()).SendAsync("NewMessage", message);
            }
        }

        public async Task MuteMicro(bool muteMicro)
        {
            var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            if (group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteMicro", new { username = Context.User.GetUsername(), mute = muteMicro });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task MuteCamera(bool muteCamera)
        {
            var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            if (group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteCamera", new { username = Context.User.GetUsername(), mute = muteCamera });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task ShareScreen(Guid roomid, bool isShareScreen)
        {
            if (isShareScreen)//true is doing share
            {
                await _shareScreenTracker.UserConnectedToShareScreen(new UserConnectionDto(Context.User.GetUsername(), roomid));
                await Clients.Group(roomid.ToString()).SendAsync("OnUserIsSharing", Context.User.GetUsername());
            }
            else
            {
                await _shareScreenTracker.UserDisconnectedShareScreen(new UserConnectionDto(Context.User.GetUsername(), roomid));
            }
            await Clients.Group(roomid.ToString()).SendAsync("OnShareScreen", isShareScreen);
            //var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
        }

        public async Task ShareScreenToUser(Guid roomid, string username, bool isShare)
        {
            var currentBeginConnectionsUser = await _presenceTracker.GetConnectionsForUser(new UserConnectionDto(username, roomid));
            if (currentBeginConnectionsUser.Count > 0)
                await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreen", isShare);
        }

        private async Task<Room> RemoveConnectionFromGroup()
        {
            var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _unitOfWork.RoomRepository.RemoveConnection(connection);
            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Fail to remove connection from room");
        }

        private async Task<Room> AddConnectionToGroup(Guid roomId)
        {
            var group = await _unitOfWork.RoomRepository.GetRoomById(roomId);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group != null)
            {
                group.Connections.Add(connection);
            }

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to add connection to room");
        }
    }
}
