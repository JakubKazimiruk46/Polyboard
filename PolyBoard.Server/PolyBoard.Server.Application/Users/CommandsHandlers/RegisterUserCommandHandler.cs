using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<User> _userManager;

    public RegisterUserCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User { UserName = request.UserName, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            if(result.Errors.Any(e => e.Code == "DuplicateUserName"))
                return new RegisterUserResult {Success = false, Error = "Username already exists" };
         
            if(result.Errors.Any(e => e.Code == "DuplicateEmail"))
                return new RegisterUserResult {Success = false, Error = "Email already exists"};


            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new RegisterUserResult {Success = false, Error = errors};
        }
        return new RegisterUserResult {Success = true};
    }
}
