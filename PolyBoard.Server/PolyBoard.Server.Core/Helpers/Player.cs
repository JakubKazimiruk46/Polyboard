using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Entities.BoardSpaces.Property;
using PolyBoard.Server.Core.Entities.GameEvents;

namespace PolyBoard.Server.Core.Helpers
{
    //To coś to pomocnicza struktura ktora bedzie trzymana w pamieci, historie gier itp bedzie przechowywac tabela user
    public sealed class Player
    {
        public User User { get; set; }
        public int PlayerNumber { get; set; } // numer gracza - który jest w kolejce
        public string Name { get; set; } = string.Empty;
        public int Balance { get; set; } = 2000;
        public List<Property> OwnedProperties { get; init; } = [];
        public List<Card> OwnedCards { get; init; } = [];
        public List<Offer> Offers { get; init; } = [];
        public bool IsInJail { get; set; }
        public bool IsBankrupt { get; set; }
    }
}
