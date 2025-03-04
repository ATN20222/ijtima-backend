using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace MeetingPlatform.Controllers
{
    public class MeetingHub : Hub
    {
        public async Task JoinMeeting(string meetingId)
        {
            if (string.IsNullOrEmpty(meetingId))
            {
                throw new HubException("Meeting ID is required.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
            await Clients.Group(meetingId).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task LeaveMeeting(string meetingId, string userName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
            await Clients.Group(meetingId).SendAsync("UserLeft", userName);
        }

        public async Task SendMessage(string meetingId, string message)
        {
            await Clients.Group(meetingId).SendAsync("ReceiveMessage", message);
        }
        public async Task SendOffer(string meetingId, object offer)
        {
            await Clients.OthersInGroup(meetingId).SendAsync("Offer", offer);
        }
        
        public async Task SendAnswer(string meetingId, object answer)
        {
            await Clients.OthersInGroup(meetingId).SendAsync("Answer", answer);
        }

        public async Task SendIceCandidate(string meetingId, object candidate)
        {
            await Clients.OthersInGroup(meetingId).SendAsync("IceCandidate", candidate);
        }
    }
}
