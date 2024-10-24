using Microsoft.AspNetCore.SignalR;

namespace PolyBoard.Server.Presentation.Hubs
{
    public class LobbyHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReciveMessage", $"{Context.ConnectionId} has joined");
        }

        public async Task NewMessage(long username, string message) =>
            await Clients.All.SendAsync("messageReceived", username, message);
    }
}
