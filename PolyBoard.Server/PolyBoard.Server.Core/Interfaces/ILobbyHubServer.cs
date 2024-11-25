namespace PolyBoard.Server.Core.Interfaces
{
    public interface ILobbyHubServer
    {
        Task GetActiveLobbies();
        Task CreateLobby(string lobbyName, string? password);
        Task JoinLobby(Guid lobbyId, string? password);
        Task StartGame();
        Task LeaveLobby();
    }
}
