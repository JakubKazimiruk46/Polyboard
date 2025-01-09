namespace PolyBoard.Server.Application.Abstractions
{
    public interface IGameEventNotifier
    {
        Task NotifyGameEventAsync(Guid gameId, string eventName, object eventBody, CancellationToken cancellationToken = default);
    }
}
