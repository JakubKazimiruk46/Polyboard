using PolyBoard.Server.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Entities
{
    public sealed class Lobby
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string LobbyName { get; set; } = string.Empty;

        public Guid AdminId { get; set; } = new Guid();

        public List<Guid> ConnectedUsers { get; set; } = new List<Guid>();

        public Lobby() { }

        public Lobby(string lobbyName, Guid adminId)
        {
            LobbyName = lobbyName;
            AdminId = adminId;

            ConnectedUsers = new List<Guid>();
            ConnectedUsers.Add(AdminId);
        }
    }
}
