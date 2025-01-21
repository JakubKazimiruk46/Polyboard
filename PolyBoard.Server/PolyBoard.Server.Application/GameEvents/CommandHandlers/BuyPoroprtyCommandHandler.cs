using MediatR;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.GameEvents.Commands;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.GameEvents.CommandHandlers
{
    internal class BuyPoroprtyCommandHandler : IRequestHandler<BuyPropertyCommand>
    {
        private readonly IRepository<GameEvent> _gameEventRepository;
        private readonly IGameEventNotifier _notifier;

        public BuyPoroprtyCommandHandler(IRepository<GameEvent> gameEventRepository, IGameEventNotifier notifier)
        {
            _gameEventRepository = gameEventRepository;
            _notifier = notifier;
        }

        public async Task Handle(BuyPropertyCommand request, CancellationToken cancellationToken)
        {
            var gameEvent = new GameEvent
            {
                Id = Guid.NewGuid(),
                Name = "BuyProperty",
                RequiresTurn = false,
                JsonBody = request,
                Game = new Game { Id = request.GameId }
            };

            await Task.WhenAll(_gameEventRepository.CreateAsync(gameEvent, cancellationToken),
                _notifier.NotifyGameEventAsync(request.GameId, "ReceiveBuyPropertyEvent", gameEvent, cancellationToken));
        }
    }
}
