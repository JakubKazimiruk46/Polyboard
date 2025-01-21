using MediatR;

namespace PolyBoard.Server.Application.GameEvents.Commands
{
    public sealed record RollDicesCommand : IRequest
    {
        public Guid GameId { get; }
        public Guid PlayerId { get; }
        public Guid TurnId { get; }
        public int Dice1 { get; }
        public int Dice2 { get; }
        public int TotalRoll => Dice1 + Dice2;

        public RollDicesCommand(Guid gameId, Guid turnId, Guid playerId, int dice1, int dice2)
        {
            GameId = gameId;
            TurnId = turnId;
            PlayerId = playerId;
            Dice1 = dice1;
            Dice2 = dice2;

            if (dice1 < 1 || dice1 > 6 || dice2 < 1 || dice2 > 6)
                throw new ArgumentException("Dice values must be between 1 and 6.");
        }
    }
}
