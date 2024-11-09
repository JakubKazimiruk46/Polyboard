using PolyBoard.Server.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyBoard.Server.Core.Interfaces
{
    public interface ILobbyService
    {
        Task<IEnumerable<Lobby>> GetLobbiesAsync();
        Task AddLobbyAsync(string lobbyName, Guid adminId);
        Task<Lobby> GetLobbyByIdAsync(Guid lobbyId);
    }

}
