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

            if(success.Success)
                return Ok(new {status = 200, message = "User register successfull"});
            
            return BadRequest(new { status = 400, message = success.Error ?? "Registration failed." });

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
        
        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromBody] EditUserProfileCommand command){
            var success = await _mediator.Send(command);

            if(success)
                return Ok(new {status = 200, message = "Profile update Succesfull"});
            
            return BadRequest(new { status = 400, message = "Failed to update profile." });
        }
    }
}
