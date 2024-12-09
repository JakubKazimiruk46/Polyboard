using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class Card : IEntity
    {
        public Guid Id { get; set; }
        public CardType Type { get; init; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsStorable { get; set; }
        public ICollection<Transaction>? InvlovedInTransactions { get; set; }
    }

}
