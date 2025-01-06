using Microsoft.AspNetCore.SignalR;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Presentation.Hubs;

namespace PolyBoard.Server.Presentation.Helpers
{
    public class SignalRGameEventNotifier : IGameEventNotifier
    {
        private readonly IHubContext<PolyBoardHub> _hubContext;

        public SignalRGameEventNotifier(IHubContext<PolyBoardHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyGameEventAsync(Guid gameId, string eventName, object eventBody, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(gameId.ToString())
                .SendAsync(eventName, eventBody, cancellationToken);
        }
    }
}
