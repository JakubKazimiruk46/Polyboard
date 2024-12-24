using MediatR;
using PolyBoard.Server.Application.Achievements.Queries;
using PolyBoard.Server.Application.DTO;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.Achievements.QueryHandlers;

public class GetAchievementsByUserIdQueryHandler : IRequestHandler<GetAchievementsByUserIdQuery, IEnumerable<UserAchievementDTO>>
{
    private readonly IRepository<UserAchievement> _userAchievementRepository;
    private readonly IRepository<Achievement> _achievementRepository;

    public GetAchievementsByUserIdQueryHandler(IRepository<UserAchievement> userAchievementRepository, IRepository<Achievement> achievementRepository)
    {
        _userAchievementRepository = userAchievementRepository;
        _achievementRepository = achievementRepository;
    }

    public async Task<IEnumerable<UserAchievementDTO>> Handle(GetAchievementsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userAchievements = await _userAchievementRepository
            .GetAsync(ua => ua.UserId == request.UserId, cancellationToken);
        
        var achievementIds = userAchievements.Select(u => u.AchievementId).Distinct();

        var achievements = await _achievementRepository
            .GetAsync(a => achievementIds.Contains(a.Id), cancellationToken);
        
        var result = userAchievements.Select(ua => new UserAchievementDTO
        {
            Progress = ua.Progress,
            Achievement = achievements
                .Where(a => a.Id == ua.AchievementId)
                .Select(a=> new AchievementDetailsDTO
                {
                    Name = a.Name,
                    Requirement = a.Requirement,
                    NumberToReach = a.NumberToReach,
                }).FirstOrDefault()
        });
        return result;
    }
}