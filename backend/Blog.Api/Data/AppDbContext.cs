using Blog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Data;

// EF Core DbContext: veritabanı tabloları ve ilişkiler burada tanımlanır
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tablolar
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kullanıcı email'leri unique olmalı
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Post → User (Author) ilişkisi
        // Restrict: kullanıcı silinirse postlar otomatik silinmez
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comment → User (Author) ilişkisi
        // Restrict: kullanıcı silinirse yorumlar otomatik silinmez
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comment → Post ilişkisi
        // Cascade: post silinirse o postun yorumları da silinir
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
