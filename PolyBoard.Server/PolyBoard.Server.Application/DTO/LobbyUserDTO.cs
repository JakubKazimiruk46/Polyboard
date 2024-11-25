namespace PolyBoard.Server.Application.DTO
{
    public sealed record LobbyUserDTO
    {
        public string Id { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsReady { get; set; } = false;
    }
}
