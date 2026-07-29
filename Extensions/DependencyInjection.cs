using Elibrary.Api.Data;
using Elibrary.Api.Repositories;
using Elibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddLibraryServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(config.GetConnectionString("Default") ?? "Data Source=elibrary.db"));

        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<BookService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<LibraryContentService>();
        services.AddScoped<BookEngagementService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<AuditService>();
        services.AddSingleton<RealtimeService>();

        return services;
    }

    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }
}
