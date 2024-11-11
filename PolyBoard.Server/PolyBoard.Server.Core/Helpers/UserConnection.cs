using System.ComponentModel.DataAnnotations.Schema;


namespace PolyBoard.Server.Core.Helpers;

[NotMapped]
public sealed class UserConnection
{
    public Guid LobbyId { get; set; }
    public Guid UserId { get; set; }
    public string? Username { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
    public string GroupString => LobbyId.ToString();
}
