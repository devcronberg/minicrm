using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MiniCrm.Services;

namespace MiniCrm.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(JwtService jwtService, IConfiguration configuration)
    {
        _jwtService = jwtService;
        _configuration = configuration;
    }

    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] ClientCredentials credentials)
    {
        var clients = _configuration.GetSection("Clients").Get<List<ClientCredentials>>();
        // test credentials
        var validClient = clients?.FirstOrDefault(c => c.ClientId == credentials.ClientId && c.ClientSecret == credentials.ClientSecret);

        if (validClient == null)
            return Unauthorized();

        var token = _jwtService.GenerateToken(credentials.ClientId);
        return Ok(new { access_token = token });
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshRequest request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            return Unauthorized();

        var clientId = principal.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
        if (string.IsNullOrEmpty(clientId))
            return Unauthorized("Invalid token: client_id claim is missing");

        var newToken = _jwtService.GenerateToken(clientId);
        return Ok(new { access_token = newToken });
    }

    [HttpPost("revoke")]
    public IActionResult RevokeToken([FromBody] RevokeRequest request)
    {
        var success = _jwtService.RevokeToken(request.Token);
        if (!success)
            return BadRequest("Invalid token");

        return Ok("Token revoked successfully");
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] ValidateRequest request)
    {
        try
        {
            var principal = _jwtService.ValidateToken(request.Token);
            return Ok(new { valid = true });
        }
        catch (SecurityTokenException)
        {
            return Ok(new { valid = false });
        }
    }
}

public class ClientCredentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public string Token { get; set; } = string.Empty;
}

public class RevokeRequest
{
    public string Token { get; set; } = string.Empty;
}

public class ValidateRequest
{
    public string Token { get; set; } = string.Empty;
}
