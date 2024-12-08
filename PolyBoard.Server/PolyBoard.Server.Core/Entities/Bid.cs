using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class Bid : IEntity
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public Game Game { get; set; }
        public GameEvent GameEvent { get; set; }
    }
}
