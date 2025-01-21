using MediatR;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.GameEvents.Commands;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.GameEvents.CommandHandlers
{
    internal class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand>
    {
        private readonly IRepository<GameEvent> _gameEventRepository;
        private readonly IRepository<Bid> _bidRepository;
        private readonly IGameEventNotifier _notifier;

        public PlaceBidCommandHandler(IRepository<GameEvent> gameEventRepository, IRepository<Bid> bidRepository, IGameEventNotifier notifier)
        {
            _gameEventRepository = gameEventRepository;
            _bidRepository = bidRepository;
            _notifier = notifier;
        }

        public async Task Handle(PlaceBidCommand request, CancellationToken cancellationToken)
        {
            var gameEvent = new GameEvent
            {
                Id = Guid.NewGuid(),
                Name = "PlaceBid",
                RequiresTurn = false,
                JsonBody = request,
                Game = new Game { Id = request.GameId }
            };

            await Task.WhenAll(_gameEventRepository.CreateAsync(gameEvent, cancellationToken),
                _notifier.NotifyGameEventAsync(request.GameId, "ReceivePlaceBidEvent", gameEvent, cancellationToken));

           await _bidRepository.CreateAsync(new Bid
           {
               Amount = request.BidAmount,
               Game = new Game {  Id = request.GameId },
               GameEvent = gameEvent,
           });
        }
    }
}
