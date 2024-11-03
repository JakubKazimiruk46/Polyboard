using PolyBoard.Server.Core.Entities.GameEvents;
using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities
{
    [NotMapped]
    public sealed class Turn : IEntity
    {
        public Guid Id { get; set; }
        public Game Game { get; set; }
        public int TurnNumber { get; set; }
        [NotMapped]
        public Player PlayerOnMove { get; set; }
        public Guid UserId { get; set; }
        public List<GameEvent> Events { get; set; } = [];
    }
}
