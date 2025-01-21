using MediatR;

namespace PolyBoard.Server.Application.GameEvents.Commands
{
    public sealed record BuyPropertyCommand : IRequest
    {
        public Guid GameId { get; }
        public Guid TurnId { get; }
        public Guid PlayerId { get; }
        public Guid PropertyId { get; }
        public int Price { get; }
    }
}
