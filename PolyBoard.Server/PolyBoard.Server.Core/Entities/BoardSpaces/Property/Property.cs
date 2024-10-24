using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities.BoardSpaces.Property
{
    //przykład konkretnego typu z tymi abstakcjami, EF doda w tabeli BoardSpace kolumne (nazwa_ktora_zdefiniujesz w OnModelCreating)
    //i dla tych obiektow bedzie wartosc Property
    public sealed class Property : BoardSpace, ICollectable
    {
        public Player? Owner { get; set; }
        public int Price { get; set; }
        public List<RentPrice> RentPrices { get; init; } = [];
        public Color Color { get; init; }

        public override void LandOn(Player player, GameSession gameSession)
        {
            throw new NotImplementedException();
        }
    }
}
