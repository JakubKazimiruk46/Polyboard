using MediatR;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.GameEvents.Commands;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.GameEvents.CommandHandlers
{
    internal class RollDicesCommandHandler : IRequestHandler<RollDicesCommand>
    {
        private readonly IRepository<GameEvent> _gameEventRepository;
        private readonly IGameEventNotifier _notifier;

        public RollDicesCommandHandler(IRepository<GameEvent> gamesRepository, IGameEventNotifier gameEventNotifier)
        {
            _gameEventRepository = gamesRepository;
            _notifier = gameEventNotifier;
        }

        public async Task Handle(RollDicesCommand request, CancellationToken cancellationToken)
        {
            var gameEvent = new GameEvent
            {
                Id = Guid.NewGuid(),
                Name = "RollDices",
                RequiresTurn = true,
                JsonBody = request,
                Game = new Game { Id = request.GameId }
            };

            await Task.WhenAll(_gameEventRepository.CreateAsync(gameEvent),
                _notifier.NotifyGameEventAsync(request.GameId, "ReceiveRollDicesEvent", gameEvent, cancellationToken));
        }
    }
}
