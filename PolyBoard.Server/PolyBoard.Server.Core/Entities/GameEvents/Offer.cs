using PolyBoard.Server.Core.Enums;
using PolyBoard.Server.Core.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities.GameEvents
{
    [NotMapped]
    public sealed class Offer : GameEvent
    {
        public Transaction? TransactionToReciver { get; set; }
        public Transaction? TransactionToSender { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        [NotMapped]
        public bool? IsAccepted { get; set; }
        public Game Game { get; set; }

        public override void Apply(GameSession session)
        {
            throw new NotImplementedException();
        }

        private Offer() { }
    }
}
