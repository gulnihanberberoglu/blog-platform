using Blog.Api.Data;
using Blog.Api.Models;
using Blog.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, PasswordService pw, JwtService jwt) : ControllerBase
{
    // Kayıt ol
    // POST: /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        // Basit input kontrolü
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email ve Password zorunlu.");

        // Aynı email ile kayıt var mı?
        var exists = await db.Users.AnyAsync(u => u.Email == email);
        if (exists) return BadRequest("Bu email zaten kayıtlı.");

        var user = new User
        {
            Email = email,
            DisplayName = req.DisplayName.Trim(),
            PasswordHash = pw.Hash(req.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Kayıt sonrası direkt token üret (fe login )
        var token = jwt.CreateAccessToken(user);

        return Ok(new AuthResponse(
            token,
            new UserDto(user.Id, user.Email, user.DisplayName)
        ));
    }

    // Giriş yap
    // POST: /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        // Email'i normalize et
        var email = req.Email.Trim().ToLowerInvariant();

        // Kullanıcıyı bul
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Güvenlik için aynı hata mesajı (email var/yok bilgisi)
        if (user is null) return Unauthorized("Email veya şifre hatalı.");

        // Hash doğrulaması
        var ok = pw.Verify(user.PasswordHash, req.Password);
        if (!ok) return Unauthorized("Email veya şifre hatalı.");

        // Giriş başarılı => token üret
        var token = jwt.CreateAccessToken(user);

        return Ok(new AuthResponse(
            token,
            new UserDto(user.Id, user.Email, user.DisplayName)
        ));
    }
}
