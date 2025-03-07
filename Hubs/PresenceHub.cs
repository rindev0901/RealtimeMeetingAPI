using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Extensions;

namespace RealtimeMeetingAPI.Hubs
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(new UserConnectionDto(Context.User.GetUsername(), 0), Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(new UserConnectionDto(Context.User.GetUsername(), 0), Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
