// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testing.Models;
using System.Security.Cryptography;
using System.Text;
using testing.Data;

namespace testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User users)
        {
            if (await _context.Users.AnyAsync(u => u.Username == users.Username))
            {
                return BadRequest("Username already exists.");
            }

            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var user = new User
            {
                Username = users.Username,
                Password = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(users.Password))),
                Salt = Convert.ToBase64String(salt)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.Username });
        }
    }
}
