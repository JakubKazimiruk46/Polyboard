using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Helpers
{
    [NotMapped]
    public class UserConnection
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsReady { get; set; } = false;
    }

}
