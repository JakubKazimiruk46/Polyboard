using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBoard.Server.Application.Users.Queries;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Presentation.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("GetById/{userId}")]
        public async Task<ActionResult<User?>> GetById(Guid userId)
        {
            var user = await _mediator.Send(new GetUserByIdQuery { Id = userId });
            return Ok(user);
        }
    }
}
