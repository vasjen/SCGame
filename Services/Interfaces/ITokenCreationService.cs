using Microsoft.AspNetCore.Identity;


namespace SCGame.Services{
    public interface ITokenCreationService
    {
        AuthenticationResponse CreateToken(IdentityUser user);
        KeyValuePair<string,string> GetUserFromToken(string Bearer);   
    }
}