namespace SCGame.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    
    private readonly GameDbConnection _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenCreationService _jwtService;

    public UserController(GameDbConnection context, UserManager<IdentityUser> userManager, ITokenCreationService jwtService)
    {
        _context = context;
        _userManager = userManager;
        _jwtService = jwtService;
    }
   
    
    [HttpGet("GetUser")]
    public async Task<ActionResult<User>> GetUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return NotFound();
        }
        return new User {UserName = user.UserName, Email = user.Email};
    }
     [HttpPost("Create")]
    public async Task<ActionResult> CreateAsync(User user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userManager.CreateAsync(
            new IdentityUser() { UserName = user.UserName, Email = user.Email },
            user.Password
        );

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        user.Password = null;
        
        return CreatedAtAction("GetUser", new { username = user.UserName }, user);
    }
    [HttpPost("BearerToken")]
    public async Task<ActionResult<AuthenticationResponse>> CreateBearerToken(AuthenticationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Bad credentials");
        }
    
        var user = await _userManager.FindByNameAsync(request.UserName);
    
        if (user == null)
        {
            return BadRequest("Bad credentials");
        }
    
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    
        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }
    
        var token = _jwtService.CreateToken(user);
    
        return Ok(token);
}   
    
}
