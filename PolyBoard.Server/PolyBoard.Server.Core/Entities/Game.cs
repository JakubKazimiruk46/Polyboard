using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public sealed class Game : IEntity
    {
        public Guid Id { get; set; }
        public GameStatus Status { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public List<User> Players { get; set; } = [];
        public List<Bid> Bids { get; set; } = [];
    }
}
