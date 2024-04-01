using BaghChal.Contracts;
using BaghChal.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BaghChal.Controllers;

[ApiController]
[Route("api")]
public class BaghChalController : ControllerBase
{
    private readonly IBoardRepository _boardRepository;
    private readonly IHubContext<BoardHub> _hubContext;

    public BaghChalController(
        IBoardRepository boardRepository, 
        IHubContext<BoardHub> hubContext)
    {
        _boardRepository = boardRepository;
        _hubContext = hubContext;
    }

    [HttpPost, Route("register")]
    public async Task<IActionResult> Register([FromBody] Username username)
    {
        var userId = await _boardRepository.RegisterUser(username.Name);
        return Ok(userId);
    }

    [HttpPost, Route("create")]
    public async Task<IActionResult> CreateBoard([FromBody] GuidWithConnection guidWithConnection)
    {
        var userId = guidWithConnection.Guid;
        var connectionId = guidWithConnection.ConnectionId;
        var piece = guidWithConnection.Piece;
        // Console.WriteLine(userId);
        var boardId = await _boardRepository.CreateBoard(userId, piece);
        await _hubContext.Groups.AddToGroupAsync(connectionId, boardId.ToString());
        return Ok(boardId);
    }
    
    [HttpPost, Route("join")]
    public async Task<IActionResult> JoinBoard([FromBody] UserWithBoard userWithBoard)
    {
        var userId = userWithBoard.UserId;
        var boardId = userWithBoard.BoardId;
        var connectionId = userWithBoard.ConnectionId;
        //Console.WriteLine(userId);
        //Console.WriteLine(boardId);
        try
        {
            var (hostId, piece) = await _boardRepository.JoinBoard(boardId, userId);
            piece = (piece == "goat") ? "tiger" : "goat";
            await _hubContext.Groups.AddToGroupAsync(connectionId, boardId.ToString());
            await _hubContext.Clients.Group(boardId.ToString()).SendAsync(
                "opponentJoined", await _boardRepository.FindName(userId));
            var hostName = await _boardRepository.FindName(hostId);
            return Ok(new
            {
                hostName, 
                piece
            });
        }
        catch (ArgumentNullException e)
        {
            return BadRequest("такой доски не существует!");
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
        catch (NotSupportedException e) // второй игрок уже подключился
        {
            return BadRequest(e.Message);
        }
    }
}