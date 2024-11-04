using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserCommandHandler(UserManager<User> userManager, IJwtProvider jwtProvider)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null) return string.Empty;
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return string.Empty;
        }
        string token = _jwtProvider.Generate(user);
        return token;
    }
}

