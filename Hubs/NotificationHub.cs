using Microsoft.AspNetCore.SignalR;

namespace DiversityPub.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinAgentGroup(string agentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"agent_{agentId}");
        }

        public async Task LeaveAgentGroup(string agentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent_{agentId}");
        }

        public async Task ForceLogoutAgent(string agentId)
        {
            await Clients.Group($"agent_{agentId}").SendAsync("ForceLogout");
        }
    }
} 