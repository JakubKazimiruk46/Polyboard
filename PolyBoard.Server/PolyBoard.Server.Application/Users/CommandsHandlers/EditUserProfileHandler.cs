using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class EditUserProfileCommandHandler : IRequestHandler<EditUserProfileCommand, bool>{

    private readonly UserManager<User> _userManager;

    public EditUserProfileCommandHandler(UserManager<User> userManager){
        _userManager = userManager;
    }

    public async Task<bool> Handle(EditUserProfileCommand request, CancellationToken cancellationToken){

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if(user == null)
            return false;
        
        user.UserName = request.UserName ?? user.UserName;
        user.Email = request.Email ?? user.Email;

        var result = await _userManager.UpdateAsync(user);

        if(!result.Succeeded)
            return false;

        if(!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword)){
            var passChangeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if(!passChangeResult.Succeeded)
                return false;
        }

        return result.Succeeded;
    }

}

