using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Entities.BoardSpaces.Property;
using PolyBoard.Server.Core.Entities.GameEvents;
using PolyBoard.Server.Core.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities.GameEvents.Auction
{
    public sealed class Auction : GameEvent
    {
        public Property AuctionedProperty { get; init; }
        public Bid? WinningBid { get; private set; }
        public List<Bid> Bids { get; private set; } = [];
        [NotMapped]
        public bool IsFinished { get; private set; } = false;
        public Game Game { get; private set; }

        public override void Apply(GameSession session)
        {
            throw new NotImplementedException();
        }

        public void PlaceBid(Bid bid)
        {
            Bids.Add(bid);
            if (bid.Amount > WinningBid?.Amount &&
                WinningBid.Player.User.Id != bid.Player.User.Id)
            {
                WinningBid = bid;
            }
        }
    }
}
