using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Exceptions
{
    public class LobbyCollectionNullException : Exception
    {
        public LobbyCollectionNullException()
            : base("Lobby collection is null.") { }

        public LobbyCollectionNullException(string message)
            : base(message) { }

        public LobbyCollectionNullException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}
