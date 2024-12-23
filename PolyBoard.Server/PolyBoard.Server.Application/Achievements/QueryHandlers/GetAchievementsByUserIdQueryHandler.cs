using MediatR;
using PolyBoard.Server.Application.Achievements.Queries;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Interfaces.Repositories;

namespace PolyBoard.Server.Application.Achievements.QueryHandlers;

public class GetAchievementsByUserIdQueryHandler : IRequestHandler<GetAchievementsByUserIdQuery, IEnumerable<UserAchievement>>
{
    private readonly IRepository<UserAchievement> _userAchievementRepository;

    public GetAchievementsByUserIdQueryHandler(IRepository<UserAchievement> userAchievementRepository)
    {
        _userAchievementRepository = userAchievementRepository;
    }

    public async Task<IEnumerable<UserAchievement>> Handle(GetAchievementsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userAchievements = await _userAchievementRepository
            .GetAsync(ua => ua.UserId == request.UserId, cancellationToken);

        return userAchievements.ToList();
    }
}