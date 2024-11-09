using System.ComponentModel.DataAnnotations.Schema;
using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities.Cards
{
    //Ta klasa jest abstrakcyjna, bo trzeba efekty zaimplementować dla konkretnych kart (idziesz do wiezienia, dostajesz kaske od kazdego itp)
    // W tabeli bedzie kolumna okreslajaca konkretny typ karty zmapowana przez EF (TPH)
    //patrz command pattern
    [NotMapped]
    public class Card : IEntity, IEventSource, ICollectable
    {
        public Guid Id { get; set; } // Klucz główny
        public CardType Type { get; init; }
        public string Name { get; set; } = string.Empty; // Nazwa karty
        public string Description { get; set; } = string.Empty; // Opis karty
        public bool IsStorable { get; set; } // Określa, czy karta może być zachowana lub użyta później
        [NotMapped]
        public Player? Owner { get; set; } // Nawigacja do gracza
        public virtual void ApplyEffect(Player player)
        {
            
        }// Abstrakcyjna metoda, która zaimplementuje konkretny efekt karty
    }

}
