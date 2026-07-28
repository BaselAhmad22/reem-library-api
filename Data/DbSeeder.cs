using Elibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                Email = "admin@elibrary.com",
                FullName = "Library Admin",
                Role = "super_admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsActive = true
            });
        }

        if (!await db.LibrarySettings.AnyAsync())
        {
            db.LibrarySettings.Add(new LibrarySettings
            {
                NameAr = "مكتبة ريم الإلكترونية",
                NameEn = "Reem Digital Library",
                TaglineAr = "اقرأ · اكتشف · تعلّم",
                TaglineEn = "Read · Discover · Learn",
                AboutAr = "مكتبة إلكترونية بسيطة تقدّم مجموعة مختارة من الكتب للتصفح والاستكشاف، مع لوحة تحكم لإدارة المحتوى بسهولة.",
                AboutEn = "A simple digital library offering a curated collection of books to browse and explore, with an admin panel for easy content management.",
                Email = "hello@elibrary.local",
                Phone = "+60196493629",
                WhatsApp = "60196493629",
                AddressAr = "كوالالمبور، ماليزيا",
                AddressEn = "Kuala Lumpur, Malaysia"
            });
        }

        if (!await db.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { NameAr = "رواية", NameEn = "Fiction", Slug = "fiction", SortOrder = 1 },
                new Category { NameAr = "تقنية", NameEn = "Technology", Slug = "technology", SortOrder = 2 },
                new Category { NameAr = "تاريخ", NameEn = "History", Slug = "history", SortOrder = 3 },
                new Category { NameAr = "تطوير ذاتي", NameEn = "Self Development", Slug = "self-development", SortOrder = 4 },
                new Category { NameAr = "علوم", NameEn = "Science", Slug = "science", SortOrder = 5 }
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();

            db.Books.AddRange(
                new Book
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Description = "A handbook of agile software craftsmanship — practical advice for writing readable, maintainable code.",
                    CoverUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop",
                    Isbn = "9780132350884",
                    PublishedYear = 2008,
                    Language = "en",
                    AvailableCopies = 3,
                    IsFeatured = true,
                    CategoryId = categories[1].Id
                },
                new Book
                {
                    Title = "قواعد العشق الأربعون",
                    Author = "إليف شافاق",
                    Description = "رواية تجمع بين قصة حب معاصرة وحكاية جلال الدين الرومي وشمس التبريزي.",
                    CoverUrl = "https://images.unsplash.com/photo-1512820538051-e318525125fd?w=400&h=600&fit=crop",
                    Isbn = "9780143118350",
                    PublishedYear = 2010,
                    Language = "ar",
                    AvailableCopies = 2,
                    IsFeatured = true,
                    CategoryId = categories[0].Id
                },
                new Book
                {
                    Title = "Sapiens",
                    Author = "Yuval Noah Harari",
                    Description = "A brief history of humankind — from cognitive revolution to the modern age.",
                    CoverUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400&h=600&fit=crop",
                    Isbn = "9780062316097",
                    PublishedYear = 2011,
                    Language = "en",
                    AvailableCopies = 4,
                    IsFeatured = true,
                    CategoryId = categories[2].Id
                },
                new Book
                {
                    Title = "Atomic Habits",
                    Author = "James Clear",
                    Description = "Tiny changes, remarkable results — an easy way to build good habits and break bad ones.",
                    CoverUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400&h=600&fit=crop",
                    Isbn = "9780735211292",
                    PublishedYear = 2018,
                    Language = "en",
                    AvailableCopies = 5,
                    IsFeatured = false,
                    CategoryId = categories[3].Id
                },
                new Book
                {
                    Title = "A Brief History of Time",
                    Author = "Stephen Hawking",
                    Description = "From the Big Bang to black holes — classic popular science for curious minds.",
                    CoverUrl = "https://images.unsplash.com/photo-1457369804613-52c61a468e7d?w=400&h=600&fit=crop",
                    Isbn = "9780553380163",
                    PublishedYear = 1988,
                    Language = "en",
                    AvailableCopies = 2,
                    IsFeatured = false,
                    CategoryId = categories[4].Id
                },
                new Book
                {
                    Title = "Designing Data-Intensive Applications",
                    Author = "Martin Kleppmann",
                    Description = "The big ideas behind reliable, scalable, and maintainable data systems.",
                    CoverUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=400&h=600&fit=crop",
                    Isbn = "9781449373320",
                    PublishedYear = 2017,
                    Language = "en",
                    AvailableCopies = 2,
                    IsFeatured = true,
                    CategoryId = categories[1].Id
                }
            );
        }

        await db.SaveChangesAsync();
    }
}
