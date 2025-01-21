using MediatR;

namespace PolyBoard.Server.Application.GameEvents.Commands
{
    public sealed record PlaceBidCommand : IRequest
    {
        public Guid GameId { get; }
        public Guid PlayerId { get; set; }
        public int BidAmount { get; }
    }
}
