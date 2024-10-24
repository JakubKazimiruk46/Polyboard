using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities.GameEvents.Auction
{
    public sealed class Bid : IEntity
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        [NotMapped]
        public Player Player { get; set; }
        public Guid UserId { get; set; }
        public Auction Auction { get; set; }
    }
}
