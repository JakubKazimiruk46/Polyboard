using MediatR;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Application.Achievements.Queries;

public class GetAchievementsByUserIdQuery : IRequest<IEnumerable<UserAchievement>>
{
    public Guid UserId { get; set; }
}