using Microsoft.AspNetCore.Mvc;
using PolyBoard.Server.Presentation.Hubs;
using PolyBoard.Server.Application.DTO;

[ApiController]
[Route("[controller]")]
public class LobbyController : ControllerBase
{
    
    [HttpGet("lobbies")]
    public ActionResult<List<LobbyListItemDTO>> GetAvailableLobbies()
    {
        var availableLobbies = PolyBoardHub.GetAvailableLobbies();
        return Ok(availableLobbies);
    }
}
