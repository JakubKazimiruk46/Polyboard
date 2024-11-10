using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Exceptions;
using PolyBoard.Server.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Helpers
{
    public sealed class LobbyService : ILobbyService
    {
        private static LobbyService? _instance;
        private static readonly object _lock = new();

        private readonly List<Lobby> _lobbies;
        private readonly List<UserConnection> _userConnections;

        public LobbyService()
        {
            _lobbies = new List<Lobby>();
            _userConnections = new List<UserConnection>();
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

        public List<Lobby> GetLobbies()
        {
            if (_lobbies == null)
                throw new LobbyCollectionNullException();

            return _lobbies;
        }

        public void AddLobby(string lobbyName, Guid adminId)
        {
            if (_lobbies == null)
                throw new LobbyCollectionNullException();

            var lobby = new Lobby(lobbyName, adminId);
            _lobbies.Add(lobby);
        }

        public Lobby? GetLobbyById(Guid? lobbyId)
        {
            if (lobbyId == null)
                return null;
            
            Lobby? lobby = _lobbies.FirstOrDefault(l => lobbyId == l.Id);
            return lobby;
        }

        public void RemoveLobbyById(Guid? lobbyId)
        {
            Lobby? lobbyToRemove = _lobbies.FirstOrDefault(l => l.Id == lobbyId);
            if (lobbyToRemove != null)
                _lobbies.Remove(lobbyToRemove);
        }

        public void ChangeLobbyAdmin(Guid lobbyId, Guid newAdminId)
        {
            Lobby? lobby = _lobbies.FirstOrDefault(l => l.Id == lobbyId);

            if (lobby != null)  
                lobby.AdminId = newAdminId;
        }

        public void ChangeLobbyAdmin(Guid? lobbyId)
        {
            Lobby? lobby = _lobbies.FirstOrDefault(l => l.Id == lobbyId);

            if (lobby != null)
            {
                IEnumerable<UserConnection> userConnectionInThisLobby = _userConnections.Where(uc => uc.LobbyId == lobbyId);
                UserConnection? newAdmin = _userConnections.FirstOrDefault(uc => uc.UserId != lobby.AdminId);

                if (newAdmin != null)
                    lobby.AdminId = newAdmin.UserId;
            }
        }
        
        public List<UserConnection> GetConnectionsByLobby(Guid? lobbyId)
        {
            if (lobbyId == null)
            {
                throw new Exception("Null in lobbyId");
            }

            List<UserConnection> userConnectionsInThisLobby = _userConnections.Where(uc => uc.LobbyId == lobbyId).ToList();
            return userConnectionsInThisLobby;
        }

        public void AddConnectionToLobby(Guid userId, string connectionId, Guid lobbyId)
        {
            var userConnection = new UserConnection
            {
                UserId = userId,
                ConnectionId = connectionId,
                LobbyId = lobbyId
            };
            _userConnections.Add(userConnection);
        }

        public Guid? GetLobbyIdByConnection(string connectionId)
        {
            var userConnection = _userConnections.FirstOrDefault(uc => uc.ConnectionId == connectionId);
            return userConnection?.LobbyId;
        }

        public void RemoveConnectionFromLobby(string connectionId)
        {
            var userConnection = _userConnections.FirstOrDefault(uc => uc.ConnectionId == connectionId);
            if (userConnection != null)
            {
                _userConnections.Remove(userConnection);
            }
        }

        public Guid? GetUserIdByConnection(string connectionId)
        {
            UserConnection? userConnection = _userConnections.FirstOrDefault(u => u.ConnectionId == connectionId);

            return userConnection?.UserId;
        }
    }
}
