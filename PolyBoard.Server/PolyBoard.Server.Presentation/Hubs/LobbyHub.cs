using Microsoft.AspNetCore.SignalR;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces;
using System.Threading.Tasks;

namespace PolyBoard.Server.Presentation.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly ILobbyService _lobbyService;

        public LobbyHub(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the server.");
        }

        public async Task JoinSpecificGameRoom(UserConnection conn)
        {
            var lobby = await _lobbyService.GetLobbyByIdAsync(conn.LobbyId);
            if (lobby == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Lobby not found.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, conn.LobbyId.ToString());

            await Clients.Caller.SendAsync("ReceiveMessage", $"Joined lobby {conn.LobbyId}");
        }
    }
}
