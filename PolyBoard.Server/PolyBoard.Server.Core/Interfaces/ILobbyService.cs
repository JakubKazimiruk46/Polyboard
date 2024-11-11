using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Helpers;

namespace PolyBoard.Server.Core.Interfaces
{
    public interface ILobbyService
    {
        List<Lobby> GetLobbies();
        void AddLobby(string lobbyName, Guid adminId);
        Lobby? GetLobbyById(Guid? lobbyId);
        void RemoveLobbyById(Guid? lobbyId);
        void ChangeLobbyAdmin(Guid lobbyId, Guid newAdminId);
        void ChangeLobbyAdmin(Guid? lobbyId);
        Guid? GetLobbyIdByConnection(string connectionId);
        void AddConnectionToLobby(Guid userId, string connectionId, Guid lobbyId);
        void RemoveConnectionFromLobby(string connectionId);
        Guid? GetUserIdByConnection(string connectionId);
        List<UserConnection> GetConnectionsByLobby(Guid? lobbyId);

    }

}
