using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Api.Services;

// JWT ayarlarını appsettings.json'dan okumak için
public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
}

// JWT üretiminden sorumlu servis
public class JwtService(IOptions<JwtOptions> opt)
{
    private readonly JwtOptions _opt = opt.Value;

    // Kullanıcı için access token oluşturur
    public string CreateAccessToken(User user)
    {
        // Token içine eklenecek bilgiler 
        var claims = new List<Claim>
        {
            // Kullanıcı ID (token'dan userId okumak için)
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            // Email bilgisi
            new(JwtRegisteredClaimNames.Email, user.Email),

            // Ekranda gösterilecek isim
            new("name", user.DisplayName)
        };

        // Secret key (imzalama için)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_opt.Key)
        );

        // Token imzalama algoritması
        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // JWT oluşturma
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8), // Token geçerlilik süresi
            signingCredentials: creds
        );

        // Token'ı string olarak döndür
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
