using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace PolyBoard.Server.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LobbyController : ControllerBase
    {
        private readonly ILobbyService _lobbyService;

        public LobbyController(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
        }

        [HttpGet("GetLobbies")]
        public async Task<IActionResult> GetLobbies()
        {
            var lobbies = await _lobbyService.GetLobbiesAsync();
            return Ok(lobbies);
        }

        [HttpPost("CreateLobby")]
        public async Task<IActionResult> CreateLobby([FromBody] LobbyCreationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.LobbyName))
            {
                return BadRequest("Lobby name cannot be empty.");
            }

            if (request.AdminId == Guid.Empty)
            {
                return BadRequest("Invalid admin ID.");
            }

            await _lobbyService.AddLobbyAsync(request.LobbyName, request.AdminId);
            return Ok();
        }
    }

    public class LobbyCreationRequest
    {
        public string? LobbyName { get; set; }
        public Guid AdminId { get; set; }
    }
}
