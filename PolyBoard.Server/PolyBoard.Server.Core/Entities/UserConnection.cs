using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Entities
{
    public sealed class UserConnection
    {
        public Guid LobbyId { get; set; }
        public string? UserId { get; set; }
    }
}
