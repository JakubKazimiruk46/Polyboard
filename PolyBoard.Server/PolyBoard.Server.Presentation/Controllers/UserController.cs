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

        [HttpPatch("edit-profile")]
        public async Task<IActionResult> EditProfile([FromBody] EditUserProfileCommand command)
        {
            if (command == null)
            {
                Console.WriteLine("EditProfile called with a null command.");
                return BadRequest(new { status = 400, message = "Invalid request body." });
            }

            try
            {
                var success = await _mediator.Send(command);

                if (success)
                {
                    Console.WriteLine($"Profile updated successfully for user {command.UserId}.");
                    return Ok(new { status = 200, message = "Profile updated successfully." });
                }

                Console.WriteLine($"Profile update failed for user {command.UserId}.");
                return BadRequest(new { status = 400, message = "Failed to update profile. Please check the provided data and try again." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile for user {command.UserId}: {ex.Message}");
                return StatusCode(500, new { status = 500, message = "An error occurred while processing the request." });
            }
        }

    }
}

