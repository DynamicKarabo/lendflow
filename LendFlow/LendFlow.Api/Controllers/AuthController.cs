using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LendFlow.Api.Controllers;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string Role);

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Dev-only: accept any email with password "password"
        if (request.Password != "password")
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var role = request.Email.Contains("admin") ? "admin" :
                   request.Email.Contains("underwriter") ? "underwriter" : "user";

        var token = GenerateJwt(request.Email, role);

        return Ok(new LoginResponse(token, request.Email, role));
    }

    private string GenerateJwt(string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("tenant_id", "00000000-0000-0000-0000-000000000001")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
