namespace Blog.Api.Models;

public record RegisterRequest(string Email, string DisplayName, string Password);
public record LoginRequest(string Email, string Password);

public record AuthResponse(string AccessToken, UserDto User);

public record UserDto(int Id, string Email, string DisplayName);

public record PostCreateRequest(string Title, string Content);
public record PostUpdateRequest(string Title, string Content);

public record PostListItemDto(int Id, string Title, string Excerpt, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, UserDto Author, int CommentCount);
public record PostDetailDto(int Id, string Title, string Content, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, UserDto Author);

public record CommentCreateRequest(string Body);
public record CommentDto(int Id, string Body, DateTime CreatedAtUtc, UserDto Author);
