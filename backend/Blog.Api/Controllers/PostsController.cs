using Blog.Api.Data;
using Blog.Api.Models;
using Blog.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController(AppDbContext db) : ControllerBase
{
    // Post listesi + arama + sayfalama
    // GET: /api/posts?search=...&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Sayfa değerlerini güvenli hale getir
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        // IQueryable: filtre ve pagination SQL'e çevrilir
        IQueryable<Post> q = db.Posts
            .Include(p => p.Author)   // Author bilgisi için
            .Include(p => p.Comments); // Comment sayısı için

        // Başlık veya içerikte arama
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(p => p.Title.ToLower().Contains(s) || p.Content.ToLower().Contains(s));
        }

        // Toplam kayıt sayısı (UI pagination için)
        var total = await q.CountAsync();

        // En güncelden sırala + sayfalama + DTO'ya çevir
        var items = await q
            .OrderByDescending(p => p.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Title,
                // Liste ekranı için kısa içerik
                p.Content.Length > 160 ? p.Content.Substring(0, 160) + "…" : p.Content,
                p.CreatedAtUtc,
                p.UpdatedAtUtc,
                new UserDto(p.Author!.Id, p.Author.Email, p.Author.DisplayName),
                p.Comments.Count
            ))
            .ToListAsync();

        return Ok(new { page, pageSize, total, items });
    }

    // Post detay
    // GET: /api/posts/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDetailDto>> Get(int id)
    {
        var p = await db.Posts
            .Include(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p is null) return NotFound();

        return Ok(new PostDetailDto(
            p.Id, p.Title, p.Content, p.CreatedAtUtc, p.UpdatedAtUtc,
            new UserDto(p.Author!.Id, p.Author.Email, p.Author.DisplayName)
        ));
    }

    // Post oluşturma (login zorunlu)
    // POST: /api/posts
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDetailDto>> Create([FromBody] PostCreateRequest req)
    {
        // Token içinden userId al
        var userId = CurrentUser.GetUserId(User);
        if (userId is null) return Unauthorized();

        var p = new Post
        {
            Title = req.Title.Trim(),
            Content = req.Content.Trim(),
            AuthorId = userId.Value
        };

        db.Posts.Add(p);
        await db.SaveChangesAsync();

        // Response DTO için author bilgisi
        var author = await db.Users.FindAsync(userId.Value);

        return CreatedAtAction(nameof(Get), new { id = p.Id }, new PostDetailDto(
            p.Id, p.Title, p.Content, p.CreatedAtUtc, p.UpdatedAtUtc,
            new UserDto(author!.Id, author.Email, author.DisplayName)
        ));
    }

    // Post güncelleme (sadece postun sahibi)
    // PUT: /api/posts/{id}
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PostUpdateRequest req)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null) return Unauthorized();

        var p = await db.Posts.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();

        // Sahiplik kontrolü
        if (p.AuthorId != userId.Value) return Forbid("Sadece kendi postunu güncelleyebilirsin.");

        p.Title = req.Title.Trim();
        p.Content = req.Content.Trim();
        p.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // Post silme (sadece postun sahibi)
    // DELETE: /api/posts/{id}
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null) return Unauthorized();

        var p = await db.Posts.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();

        // Sahiplik kontrolü
        if (p.AuthorId != userId.Value) return Forbid("Sadece kendi postunu silebilirsin.");

        db.Posts.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Kendi postlarını toplu silme (Temizle)
    // DELETE: /api/posts/clear
    [Authorize]
    [HttpDelete("clear")]
    public async Task<ActionResult<object>> ClearMyPosts()
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null) return Unauthorized();

        var myPosts = await db.Posts
            .Where(p => p.AuthorId == userId.Value)
            .ToListAsync();

        var count = myPosts.Count;

        db.Posts.RemoveRange(myPosts);
        await db.SaveChangesAsync();

        return Ok(new { deleted = count });
    }
}
