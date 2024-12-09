using PolyBoard.Server.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Helpers
{
    [NotMapped]
    public class Lobby
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string LobbyName { get; set; } = "Lobby";
        public LobbyStatus LobbyStatus { get; set; } = LobbyStatus.Waiting;
        //assuming the first user connection is associated to lobby's admin
        public List<UserConnection> Connections { get; private set; } = [];
        public int MaxPlayers { get; private set; } = 4;
        public string? Password { get; private set; }
        public bool IsPrivate => Password != null;
        public bool IsGameStarted { get; private set; } = false;
        public int CurrentTurnIndex { get; private set; } = 0;
        public List<UserConnection> TurnOrder => Connections;

        public Lobby() { }
        public Lobby(string lobbyName, int? maxPlayers = 4, string? password = null)
        {
            LobbyName = lobbyName;
            Id = Guid.NewGuid();
            LobbyStatus = LobbyStatus.Waiting;
            Connections = new List<UserConnection>();
            MaxPlayers = maxPlayers ?? 4;
            Password = password;
        }

        public bool AddConnection(UserConnection playerConnection, string? password = null)
        {
            if (IsPrivate && password != Password)
                return false;

            if (Connections.Contains(playerConnection) || Connections.Count >= MaxPlayers)
                return false;

            Connections.Add(playerConnection);
            return true;
        }

        public void RemoveConnection(UserConnection playerConnection) 
        {
            if (!Connections.Contains(playerConnection))
                return;

            Connections.Remove(playerConnection); 
        }
        public void StartGame()
        {
            if (IsGameStarted)
                throw new InvalidOperationException("Game has already started.");

            if (Connections.Count < 2)
                throw new InvalidOperationException("Not enough players to start the game.");

            if (Connections.Any(c => !c.IsReady))
                throw new InvalidOperationException("Not all players are ready.");

            IsGameStarted = true;
            CurrentTurnIndex = 0;
        }
        public UserConnection GetCurrentPlayer()
        {
            if (!IsGameStarted)
                throw new InvalidOperationException("Game has not started.");

            return TurnOrder[CurrentTurnIndex];
        }

        public void NextTurn()
        {
            if (!IsGameStarted)
                throw new InvalidOperationException("Game has not started.");

            CurrentTurnIndex = (CurrentTurnIndex + 1) % TurnOrder.Count;
        }

        public void ResetGame()
        {
            IsGameStarted = false;
            CurrentTurnIndex = 0;
        }
    }
}
