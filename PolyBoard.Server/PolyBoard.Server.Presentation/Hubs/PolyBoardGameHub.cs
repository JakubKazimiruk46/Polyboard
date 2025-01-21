using MediatR;
using Microsoft.AspNetCore.SignalR;
using PolyBoard.Server.Application.GameEvents.Commands;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Presentation.Hubs
{
    public partial class PolyBoardHub
    {
        private readonly IMediator _mediator;
        private readonly IRepository<GameEvent> _gameEventRepository;
        public PolyBoardHub(IMediator mediator, IRepository<GameEvent> gameEventRepository)
        {
            _mediator = mediator;
            _gameEventRepository = gameEventRepository;
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

        public async Task RollDices(RollDicesCommand command)
        {
            await _mediator.Send(command);
        }

        public async Task BuyProperty(BuyPropertyCommand command)
        {
            await _mediator.Send(command);
        }

        public async Task StartAuction(StartAuctionCommand command)
        {
            await _mediator.Send(command);
        }

        public async Task PlaceBid(PlaceBidCommand command)
        {
            await _mediator.Send(command);
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
                    c.IsReady
                }).ToList()
            };

            await Clients.Group(lobbyId.ToString()).SendAsync("GameStateUpdated", gameState);
        }

    }
}
