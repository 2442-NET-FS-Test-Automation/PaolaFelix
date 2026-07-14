using Library.ControllerApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]

public class AuthController : ControllerBase
{
    // Sam constructor injection as any other controller. The token stuff is just another
    // service behind and interface

    private readonly ITokenService _tokens;

    public AuthController(ITokenService tokens)
    {
        _tokens = tokens;
    }

    [HttpPost("token")]
    public ActionResult IssueToken(string userName)
    {   // Get a new token
        var userToken = _tokens.Issue(userName);
        // Return it
        return Ok();
    }
}