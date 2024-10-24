using MediatR;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.Queries
{
    public class GetUserByIdQuery : IRequest<User?>
    {
        public Guid Id { get; set; }
    }
}