using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YTU_test.Data;
using YTU_test.Models;
using YTU_test.Models.Requests;
using System.Linq;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Kullanıcı adı zaten kayıtlı." });

        var validRoles = new List<string> { "User", "Admin" };
        if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase)) // Case-insensitive karşılaştırma
        {
            return BadRequest(new { message = "Geçersiz rol. Sadece 'User' veya 'Admin' rolleri kabul edilir." });
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            RefreshToken = "",
            RefreshTokenExpiryTime = DateTime.MinValue
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kayıt başarılı." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });

        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Giriş başarılı.",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 900 
        });
    }

    
}
