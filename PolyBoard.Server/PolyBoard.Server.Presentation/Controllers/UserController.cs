using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PolyBoard.Server.Application.Users.Commands;
using PolyBoard.Server.Application.Users.Queries;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Presentation.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;

        public UserController(IMediator mediator, UserManager<User> userManager)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userManager = userManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var success = await _mediator.Send(command);
<<<<<<< HEAD
            return success ? Ok("User registered successfully.") : BadRequest("Registration failed.");
=======
            return success ? Ok(new {status=200, message="User registered successfully."}) : BadRequest(new {status=400, message="Registration failed."});
>>>>>>> 46364e61ae863ab5a163b12d05610543dc42f1ec
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var token = await _mediator.Send(command);
            return token != string.Empty ? Ok(token): Unauthorized(false);
        }
        [HttpGet("GetById/{userId}")]
        public async Task<ActionResult<User?>> GetById(Guid userId)
        {
            var user = await _mediator.Send(new GetUserByIdQuery { Id = userId });
            return Ok(user);
        }
    }
}
