using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public sealed class Transaction : IEntity
    {
        public Guid Id { get; set; }
        public User? Giver { get; set; }
        public User? Reciver { get; set; } // null indicates SYSTEM user
        public int? CashAmount { get; set; }
        public List<Card>? Cards { get; set; }
        public List<Property>? Properties { get; set; }
        public bool IsComplete { get; set; }
        public Game Game { get; set; }
        public GameEvent GameEvent { get; set; }
    }
}
