using Microsoft.AspNetCore.Identity;

namespace Blog.Api.Services;

// Şifre hashleme ve doğrulama işlemleri
public class PasswordService
{
    // ASP.NET Identity'nin güvenli password hasher'ı
    private readonly PasswordHasher<object> _hasher = new();

    // Plain password → hash (DB'ye hash kaydedilir)
    public string Hash(string password) =>
        _hasher.HashPassword(new object(), password);

    // Girilen şifre ile DB'deki hash'i karşılaştırır
    public bool Verify(string hash, string password)
    {
        var result = _hasher.VerifyHashedPassword(
            new object(),
            hash,
            password
        );

        // SuccessRehashNeeded: hash eski ama şifre doğru
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
