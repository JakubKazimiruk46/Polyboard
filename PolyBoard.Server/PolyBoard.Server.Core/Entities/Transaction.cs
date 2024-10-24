using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities
{
    public sealed class Transaction : IEntity, IEventSource
    {
        public Guid Id { get; set; }
        [NotMapped]
        public Player? Giver { get; set; }
        public Guid? GiverId { get; set; }
        [NotMapped]
        public Player? Reciver { get; set; } // null indicates SYSTEM user
        public Guid? RevicerId { get; set; }
        public int CashAmount { get; set; } = 0;
        public List<ICollectable>? Assets { get; set; }
        public bool IsComplete { get; set; }
        public Game Game { get; set; }
    }
}
