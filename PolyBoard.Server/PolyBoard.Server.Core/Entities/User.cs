using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public sealed class User : IdentityUser<Guid>, IEntity
    {
        public override Guid Id { get; set; }
        //public List<Game> Games { get; set; } = [];
    }
}
