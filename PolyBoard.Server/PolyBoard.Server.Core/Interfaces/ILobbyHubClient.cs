namespace PolyBoard.Server.Core.Interfaces
{
    public interface ILobbyHubClient
    {
        Task ReceiveMessage(string username, string message);
    }
}
