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

        public string LobbyName { get; set; } = "Room";

        public Guid AdminId { get; set; } = new Guid();

        public Lobby() { }

        public Lobby(string lobbyName, Guid adminId)
        {
            LobbyName = lobbyName;
            AdminId = adminId;
            Id = Guid.NewGuid();
        }
    }
}
