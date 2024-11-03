using MediatR;

namespace PolyBoard.Server.Application.Users.Commands;

public class RegisterUserCommand : IRequest<bool>
{
    public string Email { get; set; }
    public string Password { get; set; }
}