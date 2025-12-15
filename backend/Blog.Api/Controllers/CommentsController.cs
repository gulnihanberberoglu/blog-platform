using Blog.Api.Data;
using Blog.Api.Models;
using Blog.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:int}/[controller]")]
public class CommentsController(AppDbContext db) : ControllerBase
{
    // Post'a ait yorumları listele
    // GET: /api/posts/{postId}/comments
    [HttpGet]
    public async Task<ActionResult<List<CommentDto>>> List(int postId)
    {
        // Post var mı kontrolü
        var exists = await db.Posts.AnyAsync(p => p.Id == postId);
        if (!exists) return NotFound("Post bulunamadı.");

        var items = await db.Comments
            .Include(c => c.Author)          // Yorum sahibini göstermek için
            .Where(c => c.PostId == postId)  // Sadece ilgili postun yorumları
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new CommentDto(
                c.Id,
                c.Body,
                c.CreatedAtUtc,
                new UserDto(c.Author!.Id, c.Author.Email, c.Author.DisplayName)
            ))
            .ToListAsync();

        return Ok(items);
    }

    // Yorum ekleme (login zorunlu)
    // POST: /api/posts/{postId}/comments
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create(int postId, [FromBody] CommentCreateRequest req)
    {
        // Token içinden kullanıcıyı al
        var userId = CurrentUser.GetUserId(User);
        if (userId is null) return Unauthorized();

        // Yorum yapılacak post var mı
        var post = await db.Posts.FindAsync(postId);
        if (post is null) return NotFound("Post bulunamadı.");

        var c = new Comment
        {
            PostId = postId,
            AuthorId = userId.Value,
            Body = req.Body.Trim()
        };

        db.Comments.Add(c);
        await db.SaveChangesAsync();

        // Response için author bilgisi
        var author = await db.Users.FindAsync(userId.Value);

        return Ok(new CommentDto(
            c.Id,
            c.Body,
            c.CreatedAtUtc,
            new UserDto(author!.Id, author.Email, author.DisplayName)
        ));
    }

    // Yorum silme (sadece yorumu yazan kullanıcı)
    // DELETE: /api/posts/{postId}/comments/{commentId}
    [Authorize]
    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> Delete(int postId, int commentId)
    {
        var userId = CurrentUser.GetUserId(User);
        if (userId is null) return Unauthorized();

        // Yorum + post eşleşmesi kontrolü
        var c = await db.Comments
            .FirstOrDefaultAsync(x => x.Id == commentId && x.PostId == postId);

        if (c is null) return NotFound();

        // Sahiplik kontrolü
        if (c.AuthorId != userId.Value)
            return Forbid("Sadece kendi yorumunu silebilirsin.");

        db.Comments.Remove(c);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
