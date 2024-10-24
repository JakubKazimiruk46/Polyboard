using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities.BoardSpaces.Property
{
    // musi byc cos takiego bo w sqlach nie da sie przechowywać listy klucz - wartosc dla pojedynczej encji
    public sealed record RentPrice : IEntity
    {
        public Guid Id { get; set; }
        //let's assume NoH = 5 means that the field has a hotel
        public Property Property { get; init; }
        public int NumberOfHouses { get; init; }
        public int Price { get; init; }
    }
}
