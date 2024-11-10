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
            
            var lobby = _lobbyService.GetLobbyById(conn.LobbyId);

            if (lobby == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Lobby not found.");
                return;
            }

            _lobbyService.AddConnectionToLobby(conn.UserId, Context.ConnectionId, conn.LobbyId);

            await Groups.AddToGroupAsync(Context.ConnectionId, conn.GroupString);

            await Clients.Caller.SendAsync("ReceiveMessage", $"Joined lobby {conn.LobbyId}");
        }

        public void ChangeLobbyAdmin(Guid lobbyId, Guid newAdminId)
        {
            _lobbyService.ChangeLobbyAdmin(lobbyId, newAdminId);
        }

        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid? lobbyId = _lobbyService.GetLobbyIdByConnection(Context.ConnectionId);
            Lobby? lobby = _lobbyService.GetLobbyById(lobbyId);

            if (lobby != null && lobbyId != null)
            {
                Guid? userId = _lobbyService.GetUserIdByConnection(Context.ConnectionId);

                if (lobby.AdminId == userId)
                {
                    if (_lobbyService.GetConnectionsByLobby(lobbyId).Count > 1)
                        _lobbyService.ChangeLobbyAdmin(lobbyId);

                    else
                    {
                        _lobbyService.RemoveLobbyById(lobbyId);
                    }
                }
                Guid lobbyIdAfterConversion = (Guid)lobbyId;

                _lobbyService.RemoveConnectionFromLobby(Context.ConnectionId);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyIdAfterConversion.ToString());

                await Clients.Group(lobbyIdAfterConversion.ToString()).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has left the lobby.");
            }

            if (exception != null)
            {
                Console.WriteLine($"Error on disconnect: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }
        
    }
}
