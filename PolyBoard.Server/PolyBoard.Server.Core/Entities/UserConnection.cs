using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Entities
{
    public class UserConnection
    {
        public string Username { get; set; } = string.Empty;

        public string GameRoom { get; set; } = string.Empty;
    }
}
