using MediatR;
using Microsoft.AspNetCore.Identity;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.Users.CommandsHandlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<Achievement> _achievementRepository;
    private readonly IRepository<UserAchievement> _userAchievementRepository;

    public RegisterUserCommandHandler(UserManager<User> userManager, IRepository<Achievement> achievementRepository, IRepository<UserAchievement> userAchievementRepository)
    {
        _userManager = userManager;
        _achievementRepository = achievementRepository;
        _userAchievementRepository = userAchievementRepository;
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
        
        await InitializeUserAchievementsAsync(user, cancellationToken);
        
        return new RegisterUserResult {Success = true};
    }

    private async Task InitializeUserAchievementsAsync(User user, CancellationToken cancellationToken)
    {
        
        var achievements = await _achievementRepository.GetAllAsync();
        var userAchievements = achievements.Select(a => new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AchievementId = a.Id, 
            Progress = 0,
        }).ToList();
        
        foreach (var userAchievement in userAchievements)
        {
            await _userAchievementRepository.CreateAsync(userAchievement, cancellationToken);
        }
    }

}
