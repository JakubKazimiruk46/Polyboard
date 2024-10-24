using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities.GameEvents
{
    //Case jak z kartami, move, buy, bankrupt, jail etc Transaction chyba tez bedzie konkretna implementacja tego,
    //zalozymy ze zaakceptowac oferte/wysłać mozna w swojej turze np bo nwm jak to inaczej logicznie rozwiazac narazie xd
    public abstract class GameEvent : IEntity, IEventSource
    {
        public Guid Id { get; set; }
        public Turn Turn { get; set; }
        public virtual bool RequiresTurn { get; set; }
        [NotMapped]
        public Player? Initiator { get; set; }
        public Guid? UserId { get; set; }
        public abstract void Apply(GameSession session);

        protected GameEvent(Turn turn, bool requiresTurn, Player? initiator)
        {
            Turn = turn;
            RequiresTurn = requiresTurn;
            Initiator = initiator;
            UserId = Initiator?.User.Id;
        }

        protected GameEvent() { }
    }

}
