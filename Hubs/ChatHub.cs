using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Extensions;
using RealtimeMeetingAPI.Interfaces;
using System.Security;

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

        //public override async Task OnConnectedAsync()
        //{
        //    var httpContext = Context.GetHttpContext();
        //    var roomId = httpContext.Request.Query["roomId"].ToString();
        //    var roomIdInt = Guid.Parse(roomId);
        //    var username = Context.User.GetUsername();

        //    await _presenceTracker.UserConnected(new UserConnectionDto(username, roomIdInt), Context.ConnectionId); // Danh sách quản lý các user với các connection của từng user đó

        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomId); // Thêm roomId bằng connection cho Hub Groups quản lý
        //    await AddConnectionToGroup(roomIdInt); // Và đồng bộ vào csdl

        //    var oneUserOnline = await _unitOfWork.UserRepository.GetMemberAsync(username); // Lấy thông tin user hiện tại đang lập connection
        //    var currentUsers = await _presenceTracker.GetOnlineUsers(roomIdInt); // Lấy tất cả user đang có trong room hiện tại


        //    await Clients.Group(roomId).SendAsync("UserOnlineInGroup", oneUserOnline); // Gửi thông tin user vừa kết nối tới room của user đó


        //    await _unitOfWork.RoomRepository.UpdateCountMember(roomIdInt, currentUsers.Count); // Cập nhật số lượng user online của room hiện tại
        //    await _unitOfWork.Complete();

        //    var currentConnectionIds = await _presenceTracker.GetConnectionsForUser(new UserConnectionDto(username, roomIdInt)); // Lấy hết các connection của user hiện tại

        //    await _presenceHub.Clients.AllExcept(currentConnectionIds).SendAsync("CountMemberInGroup",
        //           new { roomId = roomIdInt, countMember = currentUsers.Count }); // Gửi message đến tất cả user có trong room hiện tại ngoại trừ user vừa tham gia và các connection của user đó

        //    //share screen user vao sau cung
        //    var userIsSharing = await _shareScreenTracker.GetUserIsSharing(roomIdInt);
        //    if (userIsSharing != null)
        //    {
        //        var currentBeginConnectionsUser = await _presenceTracker.GetConnectionsForUser(userIsSharing);
        //        if (currentBeginConnectionsUser.Count > 0)
        //            await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreenLastUser", new { usernameTo = username, isShare = true });
        //        await Clients.Caller.SendAsync("OnUserIsSharing", userIsSharing.UserName);
        //    }
        //}
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            bool isValidRoomId = Guid.TryParse(httpContext.Request.Query["roomId"].ToString(), out Guid roomId);
            if (!isValidRoomId)
            {
                throw new HubException("Invalid meeting id");
            }
            var userId = Context.User.GetUserId();

            // Thêm user mới vào danh sách online
            await _presenceTracker.UserConnected(new UserConnectionDto(userId, roomId), Context.ConnectionId);

            // Thêm user vào nhóm (group) của phòng
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            // Và đồng bộ vào csdl
            await AddConnectionToGroup(roomId);

            // Lấy thông tin user hiện tại
            var oneUserOnline = await _unitOfWork.UserRepository.GetMemberAsync(userId);

            // Lấy danh sách các user đang online trong phòng
            var currentUserIds = await _presenceTracker.GetOnlineUserIds(roomId);

            // Gửi danh sách các user đang online đến user mới
            await Clients.Caller.SendAsync("ReceiveOnlineUsers",
                (await _unitOfWork.UserRepository.GetMemberByIdsAsync(currentUserIds)).Select(u =>
                {
                    if(u.UserId == userId)
                    {
                        return GrantPermission(u, new PermissionDto
                        {
                            HasVideo = false,
                            IsMuted = true,
                            IsSelf = true,
                            IsSpeaking = false,
                        });
                    }
                    return u; 
                })
            );

            // Thông báo cho các user khác trong phòng rằng có user mới tham gia
            await Clients.Group(roomId.ToString()).SendAsync("UserOnlineInGroup", GrantPermission(oneUserOnline, new PermissionDto
            {
                HasVideo = false,
                IsMuted = true,
                IsSelf = false,
                IsSpeaking = false,
            }));

            // Cập nhật số lượng user online trong phòng
            await _unitOfWork.RoomRepository.UpdateCountMember(roomId, currentUserIds.Count);
            await _unitOfWork.Complete();

            // Lấy hết các connection của user hiện tại
            var currentConnectionIds = await _presenceTracker.GetConnectionsForUser(new UserConnectionDto(userId, roomId));

            // Gửi message đến tất cả user có trong room hiện tại ngoại trừ user vừa tham gia và các connection của user đó
            await _presenceHub.Clients.AllExcept(currentConnectionIds).SendAsync("CountMemberInGroup",
                   new { roomId, countMember = currentUserIds.Count });

            // Xử lý chia sẻ màn hình (nếu có)
            var userIsSharing = await _shareScreenTracker.GetUserIsSharing(roomId);
            if (userIsSharing != null)
            {
                var currentBeginConnectionsUser = await _presenceTracker.GetConnectionsForUser(userIsSharing);
                if (currentBeginConnectionsUser.Count > 0)
                    await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreenLastUser", new { userIdTo = userId, isShare = true });
                await Clients.Caller.SendAsync("OnUserIsSharing", userIsSharing.UserId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.GetUserId();
            var group = await RemoveConnectionFromGroup();
            var isOffline = await _presenceTracker.UserDisconnected(new UserConnectionDto(userId, group.RoomId), Context.ConnectionId);

            await _shareScreenTracker.DisconnectedByUser(userId, group.RoomId);
            if (isOffline)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.RoomId.ToString());
                var temp = await _unitOfWork.UserRepository.GetMemberAsync(userId);
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
        private MemberDto GrantPermission(MemberDto member, PermissionDto permission)
        {
            member.Permission = permission;
            return member;
        }

        public async Task MuteMicro(bool muteMicro)
        {
            var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            if (group != null)
            {
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteMicro", new { userId = Context.User.GetUserId(), isMuted = muteMicro });
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
                await Clients.Group(group.RoomId.ToString()).SendAsync("OnMuteCamera", new { userId = Context.User.GetUserId(), isMuted = muteCamera });
            }
            else
            {
                throw new HubException("group == null");
            }
        }

        public async Task ShareScreen(Guid roomid, bool isShareScreen)
        {
            var userId = Context.User.GetUserId();
            if (isShareScreen)//true is doing share
            {
                await _shareScreenTracker.UserConnectedToShareScreen(new UserConnectionDto(userId, roomid));
                await Clients.Group(roomid.ToString()).SendAsync("OnUserIsSharing", userId);
            }
            else
            {
                await _shareScreenTracker.UserDisconnectedShareScreen(new UserConnectionDto(userId, roomid));
            }
            await Clients.Group(roomid.ToString()).SendAsync("OnShareScreen", isShareScreen);
            //var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
        }

        public async Task ShareScreenToUser(Guid roomid, Guid userId, bool isShare)
        {
            var currentBeginConnectionsUser = await _presenceTracker.GetConnectionsForUser(new UserConnectionDto(userId, roomid));
            if (currentBeginConnectionsUser.Count > 0)
                await Clients.Clients(currentBeginConnectionsUser).SendAsync("OnShareScreen", isShare);
        }

        private async Task<Room> RemoveConnectionFromGroup()
        {
            var group = await _unitOfWork.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            if (group is not null)
            {
                var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                _unitOfWork.RoomRepository.RemoveConnection(connection);
                if (await _unitOfWork.Complete()) return group;
            }
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
