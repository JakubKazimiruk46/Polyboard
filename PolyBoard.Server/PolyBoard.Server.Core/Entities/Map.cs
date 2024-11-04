using System.ComponentModel.DataAnnotations.Schema;
using PolyBoard.Server.Core.Entities.BoardSpaces;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    [NotMapped]
    public sealed class Map : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; init; } = string.Empty;
        public List<BoardSpace> BoardSpaces { get; init; } = [];
        public Queue<Card> Chances { get; init; } = [];
        public Queue<Card> OtherCards { get; init; } = [];

    }
}
