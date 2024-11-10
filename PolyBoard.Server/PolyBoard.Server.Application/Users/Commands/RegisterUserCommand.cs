using System.Text.Json.Serialization;
using MediatR;

namespace PolyBoard.Server.Application.Users.Commands;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}