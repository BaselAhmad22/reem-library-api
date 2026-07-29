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
        services.AddScoped<BookPdfService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<AuditService>();
        services.AddSingleton<RealtimeService>();
        services.AddHttpClient("gutenberg", c =>
        {
            c.Timeout = TimeSpan.FromMinutes(2);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("ReemLibraryBot/1.0 (+https://reem-library-site.netlify.app)");
        });

        return services;
    }

    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var pdfs = scope.ServiceProvider.GetRequiredService<BookPdfService>();
        await DbSeeder.SeedAsync(db, pdfs);
    }
}
