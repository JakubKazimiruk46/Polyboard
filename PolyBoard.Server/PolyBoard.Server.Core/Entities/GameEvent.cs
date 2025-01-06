using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class GameEvent : IEntity
    {
        public Guid Id { get; set; }
        public bool RequiresTurn { get; set; }
        public string Name { get; set; } = string.Empty;
        public object? JsonBody { get; set; }
        public ICollection<Bid>? Bids { get; set; }
        public Game Game { get; set; }
    }
}
