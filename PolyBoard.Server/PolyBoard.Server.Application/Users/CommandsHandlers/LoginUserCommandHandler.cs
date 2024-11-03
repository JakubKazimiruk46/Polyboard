using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public LoginUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return false;

        return await _userManager.CheckPasswordAsync(user, request.Password);
    }
}

