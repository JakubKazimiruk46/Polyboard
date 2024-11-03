using System.ComponentModel.DataAnnotations.Schema;
using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities.BoardSpaces
{
    //podobnie, moze byc property, tax, jail itp
    [NotMapped]
    public abstract class BoardSpace : IEntity, IEventSource
    {
        public Guid Id { get; set; } // Klucz główny
        public string Name { get; set; } // Nazwa pola
        public int Position { get; set; } // Pozycja na planszy

        // Metoda, która definiuje efekt lądowania na tym polu
        public abstract void LandOn(Player player, GameSession gameSession);
    }
}
