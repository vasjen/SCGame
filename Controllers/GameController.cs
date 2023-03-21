

namespace SCGame.Controllers;

[ApiController]
[SwaggerTag("Controller which responsive for game process - get,create,move")]
[Route("[controller]")]
public class GameController : ControllerBase
{
   
    private readonly GameDbConnection _context;
    private readonly IGameService _gameMaster;
    private readonly ITokenCreationService _jwtService;

    public GameController(GameDbConnection context, IGameService gameMaster, ITokenCreationService jwtService)
    {
        _context = context;
        _gameMaster = gameMaster;
        _jwtService = jwtService;
    }

    [HttpGet("Get")]
    [SwaggerOperation(
    Summary = "Get five last game",
    Description = "This endpoint will return 5 last games",
    OperationId = "Get")]
    public async Task<IEnumerable<Game?>?> GetFileLastGamesAsync()
    {

        return await _context.Games
                    .AsNoTracking()
                    .Include(p => p.Board)
                        .ThenInclude(p => p.Fields)
                    .OrderByDescending(p => p.GameId)
                    .Take(5)
                    .ToListAsync();
    }
    [Authorize]
    
    [HttpPost("Create")]
    [SwaggerOperation(
    Summary = "Create new game",
    Description = "This endpoint will return a new game",
    OperationId = "Post")]
    public async Task<IActionResult> CreateGameAsync()
    {
        var user = _jwtService.GetUserFromToken(Request.Headers["Authorization"]);
        var userName = user.Key;
        var userId = user.Value;
        List<Field> TempFields = new List<Field>();
        for (int i = 1; i <= 9 ; i++){
            TempFields.Add(new Field {FieldIndex = i, FieldValue = 0});
        }

        Game newGame = new Game() {
            FirstPlayerId = userId, 
            FirstPlayerName = userName, 
            Board = new Board()
            {
                Fields = TempFields
            }
            
    
        };

        await _context.AddAsync(newGame);
        await _context.SaveChangesAsync();

        return Ok(newGame);
    }
    [HttpGet("{id}/Get")]
    [SwaggerOperation(
    Summary = "Get game",
    Description = "This endpoint will return selected game",
    OperationId = "Get")]
    public async Task<Game?> GetAsync(int id)
    {

        return await _context.Games
                    .AsNoTracking()
                    .Include(p => p.Board)
                        .ThenInclude(p => p.Fields)
                    .Where(p=> p.GameId == id)
                    .FirstOrDefaultAsync();
    }
    [Authorize]
    [HttpPut("{id}/Move")]
    [SwaggerOperation(
    Summary = "Get move in selected game",
    Description = "This endpoint will return game after make move",
    OperationId = "Put")]
    
    public async Task<IActionResult?> MoveAsync(int id, int xPos, int yPos)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid gameId");
        }
        var game = await _context.Games
                        .Include(p => p.Board)
                            .ThenInclude(p => p.Fields)
                        .Where(p => p.GameId == id)
                        .FirstOrDefaultAsync();

        if (game == null)
        {
            return BadRequest("The game doesn't exist");
        }
         if (game.Status is not GameStatus.Active)
        {
            return BadRequest("Invalid action ");
        }
        if (xPos > 3 || yPos > 3) 
        {
            return BadRequest("Invalid value");
        }

        var user = _jwtService.GetUserFromToken(Request.Headers["Authorization"]);
        var userName = user.Key;

        if (userName != game.FirstPlayerName && userName != game.SecondPlayerName)
        {
            return BadRequest("You don't have permissions in this game");    
        }


        if ( (game.Queue == false && userName != game.FirstPlayerName) 
            || 
             (game.Queue == true && userName != game.SecondPlayerName))
        {
            return BadRequest("Is not your move now");
        }
        int index = 3 * (yPos - 1) + xPos;
        
        if (game.Board.Fields[index - 1].FieldValue != 0)
        {
            return BadRequest("Incorrect move");
        } 
        
        await _gameMaster.Move(game,xPos,yPos);
   
        return Ok(game);

    }
    

    
}
