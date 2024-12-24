using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBoard.Server.Application.Achievements.Queries;

namespace PolyBoard.Server.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class AchievementController : ControllerBase
{
    private readonly IMediator _mediator;

    public AchievementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("GetByUserId/{userId}")]
    public async Task<ActionResult> GetAchievementByUserId(Guid userId)
    {
        var achievements = await _mediator.Send(new GetAchievementsByUserIdQuery { UserId = userId });
        return Ok(achievements);
    }
}