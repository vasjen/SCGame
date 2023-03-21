namespace SCGame.Controllers;

[ApiController]
[Route("[controller]")]
[SwaggerTag("Controller which responsive for user methods - creating,finding, authentication")]
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
    [SwaggerOperation(
    Summary = "Find user",
    Description = "This endpoint will return user's data if will be found",
    OperationId = "Get")]
    [SwaggerResponse(200,"User data",typeof(User))]
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
     [SwaggerOperation(
    Summary = "Create new user",
    Description = "This endpoint will create a new user and return user's data",
    OperationId = "Post")]
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
    [SwaggerOperation(
    Summary = "Get JWT Token",
    Description = "This endpoint will return JWT token for authorized user",
    OperationId = "Post")]
    [SwaggerResponse(200,"JWT Token",type: typeof(AuthenticationResponse))]
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
