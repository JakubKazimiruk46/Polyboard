namespace PolyBoard.Server.Application.DTO
{
    public class LobbyCreationRequest
    {
        public string? LobbyName { get; set; }
        public Guid AdminId { get; set; }
    }
}
