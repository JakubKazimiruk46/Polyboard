using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Helpers;

namespace PolyBoard.Server.Application.DTO
{
    public record LobbyListItemDTO
    {
        public Guid Id { get; set; }
        public string LobbyName { get; set; } = string.Empty;
        public LobbyStatus Status { get; set; }
        public int ConnectedUserCount { get; set; }
        public bool IsPrivate { get; set; }

        public LobbyListItemDTO(Lobby lobby)
        {
            Id = lobby.Id;
            LobbyName = lobby.LobbyName;
            Status = lobby.LobbyStatus;
            ConnectedUserCount = lobby.Connections.Count;
            IsPrivate = lobby.IsPrivate;
        }
    }
}
