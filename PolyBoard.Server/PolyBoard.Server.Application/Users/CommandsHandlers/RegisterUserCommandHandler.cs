using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public RegisterUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User { UserName = request.UserName, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create user: {errors}");
        }
        return result.Succeeded;
    }
}
