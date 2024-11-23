using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using testing.Models;
using System.Security.Cryptography;
using testing.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] User user)
    {

        var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == user.Username);
        if (existingUser == null)
        {
            return Unauthorized("create new account.");
        }

        var storedHash = Convert.FromBase64String(existingUser.Password); 
        var storedSalt = Convert.FromBase64String(existingUser.Salt); 

        using var hmac = new HMACSHA512(storedSalt); 
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(user.Password));

        if (!computedHash.SequenceEqual(storedHash)) 
        {
            return Unauthorized("Invalid username or password.");
        }

        if (string.IsNullOrEmpty(existingUser.Username))
        {
            return Unauthorized("Invalid username.");
        }

        // Generate JWT token
        var token = GenerateJwtToken(existingUser.Username);
        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? ""));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
