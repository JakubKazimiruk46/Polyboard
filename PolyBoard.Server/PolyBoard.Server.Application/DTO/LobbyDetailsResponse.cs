using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Helpers;

namespace PolyBoard.Server.Application.DTO
{
    public class LobbyDetailsResponse
    {
        public Lobby? Lobby { get; set; }
        public List<UserConnection> ConnectedUsers { get; set; } = new List<UserConnection>();
    }
}
