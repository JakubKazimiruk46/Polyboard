using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    /// <summary>
    /// This class is a representation of a Player turn in database
    /// </summary>
    public sealed class Turn : IEntity
    {
        public Guid Id { get; set; }
        public Game Game { get; set; }
        public int Number { get; set; }
        public User PlayerOnMove { get; set; }
        public ICollection<GameEvent> Events { get; set; } = [];
    }
}
