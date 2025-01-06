using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rifki_Technical_Assessment.Data;
using Rifki_Technical_Assessment.Helper;
using Rifki_Technical_Assessment.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Rifki_Technical_Assessment.Models.DTOs;

namespace Rifki_Technical_Assessment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductController> _logger;

        public AuthController(AppDbContext context, IConfiguration configuration, ILogger<ProductController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                _logger.LogInformation("Received request to create a user: {Username}", registerRequest.Username);
                if (await _context.Users.AnyAsync(u => u.Username == registerRequest.Username))
                {
                    _logger.LogInformation("Username already exists: {Username}", registerRequest.Username);
                    return BadRequest("Username already exists.");
                }
                    
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = registerRequest.Username,
                    Password = (string)HashPassword(registerRequest.Password),
                    Email = registerRequest.Email,
                    Token = string.Empty,
                    CreatedDate = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while Registration");
                return StatusCode(500, "Internal server error while fetching product");
            }
        }

        

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest userRequest)
        {
            try
            {
                _logger.LogInformation("Received request to Login: {Username}", userRequest.Username);
                if (string.IsNullOrEmpty(userRequest.Password))
                {
                    _logger.LogInformation("Password is required: {Username}", userRequest.Username);
                    return Unauthorized("Password is required.");
                }

                var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == userRequest.Username);

                if (user == null || HashPassword(userRequest.Password).ToString() != user.Password)
                {
                    _logger.LogInformation("Password is required: {Username}", userRequest.Username);
                    return Unauthorized("Invalid credentials.");
                }

                var token = GenerateJwtToken(user);
                user.Token = token;
                user.ExpiredToken = DateTime.Now.AddDays(1);
                user.ModifiedDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var encryptionHelper = new EncryptionHelper(GenerateRandomBytes(32), GenerateRandomBytes(16));
                string email = encryptionHelper.Encrypt(user.Email);

                return Ok(new { token, email });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while login");
                return StatusCode(500, "Internal server error while fetching product");
            }

            
        }

        private byte[] GenerateRandomBytes(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[length];
                rng.GetBytes(bytes);
                return bytes;
            }
        }

        private object HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var aa = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
