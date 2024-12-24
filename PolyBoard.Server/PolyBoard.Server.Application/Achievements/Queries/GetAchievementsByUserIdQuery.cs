using MediatR;
using PolyBoard.Server.Application.DTO;

namespace PolyBoard.Server.Application.Achievements.Queries;

public class GetAchievementsByUserIdQuery : IRequest<IEnumerable<UserAchievementDTO>>
{
    public Guid UserId { get; set; }
}