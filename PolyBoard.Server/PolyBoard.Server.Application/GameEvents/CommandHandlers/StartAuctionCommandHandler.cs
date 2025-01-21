using MediatR;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.GameEvents.Commands;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.GameEvents.CommandHandlers
{
    internal class StartAuctionCommandHandler : IRequestHandler<StartAuctionCommand>
    {
        private readonly IRepository<GameEvent> _gameEventRepository;
        private readonly IGameEventNotifier _notifier;

        public StartAuctionCommandHandler(IRepository<GameEvent> repository, IGameEventNotifier notifier)
        {
            _gameEventRepository = repository;
            _notifier = notifier;
        }

        public async Task Handle(StartAuctionCommand request, CancellationToken cancellationToken)
        {
            var gameEvent = new GameEvent
            {
                Id = Guid.NewGuid(),
                Name = "StartAuction",
                RequiresTurn = false,
                JsonBody = request,
                Game = new Game { Id = request.GameId }
            };

            await Task.WhenAll(_gameEventRepository.CreateAsync(gameEvent, cancellationToken),
                _notifier.NotifyGameEventAsync(request.GameId, "ReceiveStartEvent", gameEvent, cancellationToken));
        }
    }
}
