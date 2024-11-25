using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PolyBoard.Server.Presentation.Hubs;
using PolyBoard.Server.Application.DTO;

[ApiController]
public class LobbyController : ControllerBase
{
    private readonly IHubContext<LobbyHub> _hubContext;

    public LobbyController(IHubContext<LobbyHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpGet("available")]
    public ActionResult<List<LobbyListItemDTO>> GetAvailableLobbies()
    {
        var availableLobbies = LobbyHub.GetAvailableLobbies();
        return Ok(availableLobbies);
    }
}
