using Microsoft.AspNetCore.Mvc;
using PolyBoard.Server.Application.DTO;
using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Core.Helpers;
using PolyBoard.Server.Core.Interfaces;


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
        public IActionResult GetLobbies()
        {
            var lobbies = _lobbyService.GetLobbies();
            return Ok(lobbies);
        }

        [HttpPost("CreateLobby")]
        public IActionResult CreateLobby([FromBody] LobbyCreationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.LobbyName == null)
                throw new ArgumentNullException(nameof(request.LobbyName));

            _lobbyService.AddLobby(request.LobbyName, request.AdminId);
            return Ok();
        }

        [HttpGet("GetLobby")]
        public IActionResult GetLobby(Guid lobbyId)
        {
            Lobby? lobby = _lobbyService.GetLobbyById(lobbyId);
            List<UserConnection> connectedUsers = _lobbyService.GetConnectionsByLobby(lobbyId);

            var response = new LobbyDetailsResponse
            {
                Lobby = lobby,
                ConnectedUsers = connectedUsers
            };

            return Ok(response);
        }

    }
}
