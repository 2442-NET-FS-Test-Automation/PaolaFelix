using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Library.ControllerApi.Services;

// logic for token issuance lives here - any service controller or other service
// that needs a JWT calls this code.

public class TokenService : ITokenService
{
    private readonly string _key;

    // This is a temp stand-in that WILL be removed - it's going to stand in for seeding main accounts.
    // We will add a users table with some admin accounts tomorrow - for true auth. This is just for AuthZ demo.
    private static readonly Dictionary<string, string> Roles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ada"] = "admin"
        };

    public TokenService(IConfiguration config)
    {
        // We probably want to avoid hardcoding the basics of our key
        // We can always add it to appsettings.Development.json and treat it as a secret
        // We probably want to then add that file to the .gitignore. Same logic as an .env file.
        _key = config["Jwt:Key"];
    }

    // Method for token inssuance. Validation lives in Program.cs
    // This token, once the front end has it , gets appended to every
    // http request. For some endpoints, we will validate the token, and if the user isn't authorized to do
    // a given action we send back 401 unauthorized
    public string Issue(string user)
    {
        // Sign the token with a symmetric key 
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTFB.GetBytes(_key)), SecurityAlgorithms.HmacSha256);

        // If user = "ada they get admin, otherwise they get "consumer"
        var role = Roles.GetValueOrDefault(user, "consumer");

        // Once we have creds we can register clams
        // things like user role. We can also give the key an expiration
        var token = new JwtSecurityToken("library-fulfillment", "library-fulfillment-clients",
            new[] { new Claim(ClaimTypes.Name, user), new Claim(ClaimTypes.Role, role) },
            expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}