using Elibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LibrarySettings> LibrarySettings => Set<LibrarySettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.FullName).HasMaxLength(200);
            e.Property(x => x.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.NameAr).HasMaxLength(200);
            e.Property(x => x.NameEn).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(200);
        });

        modelBuilder.Entity<Book>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Author).HasMaxLength(200);
            e.Property(x => x.Isbn).HasMaxLength(50);
            e.Property(x => x.Language).HasMaxLength(20);
            e.Property(x => x.CoverUrl).HasMaxLength(1000);
            e.HasOne(x => x.Category)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.Property(x => x.Action).HasMaxLength(100);
            e.Property(x => x.EntityType).HasMaxLength(100);
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
