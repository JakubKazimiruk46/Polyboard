using System.ComponentModel.DataAnnotations.Schema;


namespace PolyBoard.Server.Core.Helpers;

[NotMapped]
public class UserConnection
{
    public string Username { get; set; } = string.Empty;

    public string GameRoom { get; set; } = string.Empty;
}
