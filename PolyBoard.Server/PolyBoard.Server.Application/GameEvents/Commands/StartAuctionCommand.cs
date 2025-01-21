using MediatR;

namespace PolyBoard.Server.Application.GameEvents.Commands
{
    public sealed record StartAuctionCommand : IRequest
    {
        public Guid GameId { get; }
        public Guid PropertyId { get; }
    }
}
