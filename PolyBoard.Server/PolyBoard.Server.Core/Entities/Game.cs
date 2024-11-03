using System.ComponentModel.DataAnnotations.Schema;
using PolyBoard.Server.Core.Entities.GameEvents;
using PolyBoard.Server.Core.Entities.GameEvents.Auction;
using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    [NotMapped]
    public sealed class Game : IEntity
    {
        public Guid Id { get; set; }
        public GameStatus Status { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public Map Map { get; set; }
        //N:N
        public List<User> User { get; set; }
        public IList<Turn> Turns { get; set; }
        public IList<Auction> Auctions { get; set; }
        public IList<Offer> Offers { get; set; }
        public IList<Transaction> Transactions { get; set; }
    }
}
