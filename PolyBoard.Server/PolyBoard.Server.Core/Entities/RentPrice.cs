using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class RentPrice : IEntity
    {
        public Guid Id { get; set; }
        public int NumberOfHouses { get; set; }
        public int Price { get; set; }
        public Property Property { get; set; }
    }
}
