
namespace SCGame.Controllers;

[ApiController]
[Route("[controller]")]
[SwaggerTag("Controller which responsive for inviting mechanism for selected game - creating,sending, accepting or declining")]
public class InviteController : ControllerBase
{   
    
    private readonly GameDbConnection _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenCreationService _jwtService;
    private readonly IPlayRoomHub _hubContext;
    private readonly PlayRoomRegistry _registry;

    public InviteController(GameDbConnection context, UserManager<IdentityUser> userManager, ITokenCreationService jwtService, IPlayRoomHub hubContext, PlayRoomRegistry registry)
    {

        _context = context;
        _userManager = userManager;
        _jwtService = jwtService;
        _hubContext = hubContext;
        _registry = registry;
    }

    [Authorize]
    [HttpPost("Create")]
    [SwaggerOperation(
    Summary = "Create invite",
    Description = "This endpoint will return a new invite for the selected game",
    OperationId = "Post")]
    [SwaggerResponse(200,"Invite is created", typeof(Invite))]
    public async Task<ActionResult<Invite>> CreateInviteAsync(int id)
    {
       if (!ModelState.IsValid)
        {
            return BadRequest("Invalid gameId");
        }
        var game = await _context.Games
                        .AsNoTracking()
                        .Where(p => p.GameId == id)
                        .FirstOrDefaultAsync();
         if (game == null)
        {
            return BadRequest("The game doesn't exist");
        }
        
        var userName = _jwtService.GetUserFromToken(Request.Headers["Authorization"]).Key;
       
        Invite newInvite = new Invite() {GameId = game.GameId, FirstPlayer = userName};
        await _context.AddAsync(newInvite);
        await _context.SaveChangesAsync();
        RoomRequest room = new(userName+"_room"+game.GameId);
         _registry.CreateRoom(room.Room);


        return Ok(newInvite);
    }
    [Authorize]
    [HttpPut("{id}/Send")]
    [SwaggerOperation(
    Summary = "Send invite",
    Description = "This endpoint will send a selected invite to user",
    OperationId = "Put")]
    [SwaggerResponse(200)]
    public async Task<IActionResult> SendInviteAsync(int id, string userName)
    {   
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");
        }
        var invite = await _context.Invites
                        .Where(p => p.InviteId == id)
                        .Include(p => p.Game)
                        .FirstOrDefaultAsync();

        if (invite == null || invite.Game.Status is not GameStatus.New)
        {
            return BadRequest("Invalid invite");
        }
        var user = await _userManager.FindByNameAsync(userName);
        
        if (user == null)
        {
            return BadRequest("This user doesn't exist");
        }

        invite.SecondPlayer = user.UserName;
        invite.Game.Status = GameStatus.Waiting;

        await _context.SaveChangesAsync();
        


        return Ok();
    }
    [Authorize]
    [HttpPut("{id}/Accept")]
    [SwaggerOperation(
    Summary = "Accept income inviting",
    Description = "This endpoint will return received invite and start the game",
    OperationId = "Put")]
    [SwaggerResponse(200,"Invite accepted, the game is started", type: typeof(Invite))]
    public async Task<IActionResult> AcceptInviteAsync(int id)
    {
      if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");
        }
        var invite = await _context.Invites
                            .Where(p => p.InviteId == id)
                                .Include(p => p.Game)
                            .FirstOrDefaultAsync();

        if (invite == null || invite.Game.Status is not GameStatus.Waiting)
        {
            return BadRequest("Invalid invite");
        }
        var user = _jwtService.GetUserFromToken(Request.Headers["Authorization"]);
        var userName = user.Key;
        var userId = user.Value;

        if (userName != invite.SecondPlayer)
        {
            return BadRequest("You don't have permissions");
        }
        invite.Game.SecondPlayerId = userId;
        invite.Game.SecondPlayerName = userName;
        invite.Game.Status = GameStatus.Active;
        invite.Game.Queue = false;
        invite.Game.Moves=0;
        await _context.SaveChangesAsync();



        return Ok(invite);
    }
    [Authorize]
    [HttpDelete("{id}/Decline")]
    [SwaggerOperation(
    Summary = "Reject invite",
    Description = "This endpoint will decline a received invite and delete him",
    OperationId = "Delete")]
    [SwaggerResponse(200,"Invite #{id} was declined and removed from database")]
    public async Task<IActionResult> DeclineInviteAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");
        }
        var invite = await _context.Invites
                            .Where(p => p.InviteId == id)
                            .FirstOrDefaultAsync();

        if (invite == null || invite.Game.Status is not GameStatus.Waiting)
        {
            return BadRequest("Invalid invite");
        }
        var user = _jwtService.GetUserFromToken(Request.Headers["Authorization"]);
        var userName = user.Key;
        if (invite.SecondPlayer != userName || invite.FirstPlayer != userName)
        {
            return BadRequest("You don't have permissions");
        }

        _context.Remove(invite);
        await _context.SaveChangesAsync();

        return Ok($"Invite #{id} was declined and removed from database");
    }
    
    
}
