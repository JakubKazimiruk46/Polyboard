using MediatR;
using PolyBoard.Server.Application.Users.Queries;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;


namespace PolyBoard.Server.Application.Users.QueryHandlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
    {
        private readonly IRepository<User> _userRepository; // or concrete interface IUserRepository if a custom method is needed

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, u => new User
            {
                Id = u.Id,
                //Games = u.Games,
                //etc, needed fields to select
            }, 
            cancellationToken);

            if (user == null)
            {
                return null;
            }

            return new User // map to DTO if needed (preffered!) or simply return the user variable here
            {
                Id = user.Id,
                // Map other properties
            };
        }
    }
}
