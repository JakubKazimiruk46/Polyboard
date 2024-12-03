using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Abstractions;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class EditUserProfileCommandHandler : IRequestHandler<EditUserProfileCommand, bool>
{
    private readonly UserManager<User> _userManager;

    public EditUserProfileCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                Console.WriteLine($"User with ID {request.UserId} not found.");
                return false;
            }

            user.UserName = request.UserName ?? user.UserName;
            user.Email = request.Email ?? user.Email;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                Console.WriteLine($"Failed to update profile for user {request.UserId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return false;
            }

            if (!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword))
            {
                var passChangeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!passChangeResult.Succeeded)
                {
                    Console.WriteLine($"Failed to change password for user {request.UserId}. Errors: {string.Join(", ", passChangeResult.Errors.Select(e => e.Description))}");
                    return false;
                }
            }

            Console.WriteLine($"Profile successfully updated for user {request.UserId}.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while updating profile for user {request.UserId}: {ex.Message}");
            return false;
        }
    }
}
