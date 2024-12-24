using Microsoft.AspNetCore.SignalR;
using PolyBoard.Server.Application.DTO;
using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Helpers;
using System.Collections.Concurrent;

namespace PolyBoard.Server.Presentation.Hubs
{
    public class LobbyHub : Hub
    {
        private static readonly ConcurrentDictionary<Guid, Lobby> _lobbies = new();

        public static List<LobbyListItemDTO> GetAvailableLobbies()
        {
            var availableLobbies = _lobbies
                .Where(l => l.Value.LobbyStatus == LobbyStatus.Waiting)
                .Select(lobby => new LobbyListItemDTO(lobby.Value))
                .ToList();

            return availableLobbies;
        }

        public async Task SendLobbyDetails(Guid lobbyId, string? password = null)
        {
            var lobby = _lobbies.FirstOrDefault(l => l.Value.Id.Equals(lobbyId)).Value;

            if (lobby == null || lobby.Password != password)
                return;

            var lobbyDetails = new
            {
                id = lobby.Id,
                lobbyName = lobby.LobbyName,
                status = lobby.LobbyStatus,
                connectedUsers = lobby.Connections.Select(c => new LobbyUserDTO
                {
                    Id = c.UserId,
                    ConnectionId = c.ConnectionId,
                    Username = c.Username,
                    IsReady = c.IsReady
                }).ToList()
            };

            await Clients.Caller.SendAsync("ReceiveLobbyDetails", lobbyDetails);
        }


        public async Task CreateLobby(string lobbyName, int? maxPlayers = 6, string? password = null)
        {
            if (password == string.Empty)
                password = null;

            var lobby = new Lobby(lobbyName, maxPlayers, password);
            _lobbies[lobby.Id] = lobby;

            var userConnection = new UserConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = Context.UserIdentifier ?? Guid.NewGuid().ToString()
            };
            if(!lobby.AddConnection(userConnection, password))
            {
                await Clients.Caller.SendAsync("Error", "Failed Joinin Lobby! Try JoinLobby function...");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, lobby.Id.ToString());
            await Clients.Caller.SendAsync("LobbyCreated", lobby.Id);
        }

        public async Task JoinLobby(Guid lobbyId, string? password = null)
        {
            var lobby = _lobbies.FirstOrDefault(l => l.Value.Id == lobbyId).Value;

            if (lobby == null)
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found");
                return;
            }

            if(lobby.Password != password)
            {
                await Clients.Caller.SendAsync("Error", "Wrong password!");
                return;
            }

            if(lobby.Connections
                .Where(uc => uc.ConnectionId == Context.ConnectionId)
                .Any())
            {
                return;
            }

            var userConnection = new UserConnection
            {
                UserId = Context.UserIdentifier ?? Guid.NewGuid().ToString(),
                ConnectionId = Context.ConnectionId,
                Username = Context.UserIdentifier ?? "User",
                IsReady = false
            };

            if(!lobby.AddConnection(userConnection, password))
            {
                await Clients.Caller.SendAsync("Error", "Failed joining lobby.");
                return;
            }


            await SendLobbyDetails(lobbyId, password);

            await Clients.Group(lobbyId.ToString()).SendAsync("UserJoinedLobby", new LobbyUserDTO
            {
                Id = userConnection.UserId,
                ConnectionId = userConnection.ConnectionId,
                Username = userConnection.Username,
                IsReady = userConnection.IsReady
            });

            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId.ToString());
        }


        public async Task LeaveLobby(Guid lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }

            var userConnection = lobby.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (userConnection == null)
            {
                await Clients.Caller.SendAsync("Error", "User not in lobby.");
                return;
            }

            lobby.RemoveConnection(userConnection);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId.ToString());
            await Clients.Group(lobbyId.ToString()).SendAsync("UserLeft", Context.UserIdentifier);

            if (!lobby.Connections.Any())
            {
                _lobbies.TryRemove(lobbyId, out _);
            }
            else if (lobby.Connections.First() == userConnection)
            {
                var newAdmin = lobby.Connections.First();
                await Clients.Group(lobbyId.ToString()).SendAsync("AdminChanged", newAdmin.UserId);
            }
        }

        public async Task ChangeLobbyStatus(Guid lobbyId, LobbyStatus status)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }

            lobby.LobbyStatus = status;
            await Clients.Group(lobbyId.ToString()).SendAsync("LobbyStatusChanged", status);
        }

        public async Task SetUserReady(Guid lobbyId, bool isReady)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }

            var userConnection = lobby.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (userConnection == null)
            {
                await Clients.Caller.SendAsync("Error", "User not in lobby.");
                return;
            }

            userConnection.IsReady = isReady;
            await Clients.Group(lobbyId.ToString()).SendAsync("UserReadyStatusChanged", Context.UserIdentifier, isReady);
        }

        public async Task PromoteToAdmin(Guid lobbyId, string newAdminId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }

            var newAdmin = lobby.Connections.FirstOrDefault(c => c.UserId == newAdminId);
            if (newAdmin == null)
            {
                await Clients.Caller.SendAsync("Error", "New admin not found in the lobby.");
                return;
            }

            lock (lobby.Connections)
            {
                lobby.Connections.Remove(newAdmin);
                lobby.Connections.Insert(0, newAdmin);
            }

            await Clients.Group(lobbyId.ToString()).SendAsync("AdminChanged", newAdminId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var (lobbyId, lobby) in _lobbies)
            {
                var userConnection = lobby.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
                if (userConnection != null)
                {
                    lobby.RemoveConnection(userConnection);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId.ToString());
                    await Clients.Group(lobbyId.ToString()).SendAsync("UserDisconnected", Context.UserIdentifier);

                    if (!lobby.Connections.Any())
                    {
                        _lobbies.TryRemove(lobbyId, out _);
                    }
                    else if (lobby.Connections.First() == userConnection)
                    {
                        var newAdmin = lobby.Connections.First();
                        await Clients.Group(lobbyId.ToString()).SendAsync("AdminChanged", newAdmin.UserId);
                    }

                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task StartGame(Guid lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }
            var admin = lobby.Connections.FirstOrDefault();
            if (admin == null || admin.ConnectionId != Context.ConnectionId)
            {
                await Clients.Caller.SendAsync("Error", "Only the admin can start the game.");
                return;
            }
            
            if (lobby.Connections.Count < 2)
            {
                await Clients.Caller.SendAsync("Error", "Not enough players in the lobby. At least 2 players are required.");
                return;
            }
            if (!lobby.Connections.All(c => c.IsReady))
            {
                await Clients.Caller.SendAsync("Error", "Not all players are ready.");
                return;
            }
            
            await ChangeLobbyStatus(lobbyId, LobbyStatus.InGame);
            
            lobby.StartGame();
            await Clients.Group(lobbyId.ToString()).SendAsync("GameStarted", lobby.GetCurrentPlayer().UserId);
        }
        public async Task GetCurrentTurn(Guid lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }

            if (lobby.LobbyStatus != LobbyStatus.InGame)
            {
                await Clients.Caller.SendAsync("Error", "Game not started.");
                return;
            }

            var currentPlayer = lobby.GetCurrentPlayer();
            await Clients.Caller.SendAsync("CurrentTurn", new 
            {
                userId = currentPlayer.UserId,
                username = currentPlayer.Username
            });
        }
        public async Task EndTurn(Guid lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                await Clients.Caller.SendAsync("Error", "Lobby not found.");
                return;
            }

            if (lobby.LobbyStatus != LobbyStatus.InGame)
            {
                await Clients.Caller.SendAsync("Error", "Game not started.");
                return;
            }

            var currentPlayer = lobby.GetCurrentPlayer();
            if (currentPlayer.ConnectionId != Context.ConnectionId)
            {
                await Clients.Caller.SendAsync("Error", "Not your turn.");
                return;
            }

            lobby.NextTurn();
            await Clients.Group(lobbyId.ToString()).SendAsync("TurnChanged", lobby.GetCurrentPlayer().UserId);
        }
        private async Task NotifyGameState(Guid lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                return;

            var gameState = new
            {
                CurrentPlayerId = lobby.GetCurrentPlayer().UserId,
                PlayerOrder = lobby.Connections.Select(c => new
                {
                    c.UserId,
                    c.Username,
                    IsReady = c.IsReady
                }).ToList()
            };

            await Clients.Group(lobbyId.ToString()).SendAsync("GameStateUpdated", gameState);
        }
        
    }
}
