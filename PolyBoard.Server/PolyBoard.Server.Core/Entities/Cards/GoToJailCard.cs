using PolyBoard.Server.Core.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBoard.Server.Core.Entities.Cards;

[NotMapped]
public sealed class GoToJailCard : Card
{

    public override void ApplyEffect(Player player)
    {
        throw new NotImplementedException();
    }
}