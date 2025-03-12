using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Extensions;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Hubs
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        private readonly IUnitOfWork _unitOfWork;
        public PresenceHub(PresenceTracker tracker, IUnitOfWork unitOfWork)
        {
            _tracker = tracker;
            _unitOfWork = unitOfWork;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.GetUserId();
            var isOnline = await _tracker.UserConnected(new UserConnectionDto(userId, null), Context.ConnectionId);
            await SendUserOnlineForGroups(Context.User.GetUserId(), isOnline);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.GetUserId();
            var isOffline = await _tracker.UserDisconnected(new UserConnectionDto(userId, null), Context.ConnectionId);
            await SendUserOnlineForGroups(userId, isOffline);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendUserOnlineForGroups(Guid userId, bool isOnline)
        {
            var listGroupUserJoined = await _unitOfWork.RoomRepository.GetRoomsForUser(userId);
            await Clients.Groups(listGroupUserJoined).SendAsync("OnUserOnline", new { userId , isOnline });
        }
    }
}
