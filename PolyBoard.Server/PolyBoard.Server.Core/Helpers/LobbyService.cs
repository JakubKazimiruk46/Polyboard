using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Exceptions;
using PolyBoard.Server.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Helpers
{
    public sealed class LobbyService : ILobbyService
    {
        private static LobbyService? _instance;
        private static readonly object _lock = new(); 
        private readonly List<Lobby> _lobbies;

        private LobbyService()
        {
            _lobbies = new List<Lobby>();
        }

        public static LobbyService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LobbyService();
                    }
                }
            }

            return _instance;
        }

        public Task<IEnumerable<Lobby>> GetLobbiesAsync()
        {
            if (_lobbies == null)
                throw new LobbyCollectionNullException();

            return Task.FromResult((IEnumerable<Lobby>)_lobbies);
        }

        public Task AddLobbyAsync(string lobbyName, Guid adminId)
        {
            if (_lobbies == null)
                throw new LobbyCollectionNullException();

            var lobby = new Lobby(lobbyName, adminId);
            _lobbies.Add(lobby);

            return Task.CompletedTask;
        }

        public Task<Lobby> GetLobbyByIdAsync(Guid lobbyId)
        {
            Lobby? lobby = _lobbies.FirstOrDefault(l => lobbyId == l.Id);

            if (lobby != null)
                return Task.FromResult(lobby);

            return Task.FromResult(new Lobby());
        }
    }
}
