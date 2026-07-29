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
            await db.SaveChangesAsync();
        }

        if (!await db.LibrarySettings.AnyAsync())
        {
            db.LibrarySettings.Add(new LibrarySettings
            {
                NameAr = "مكتبة ريم الإلكترونية",
                NameEn = "Reem Digital Library",
                TaglineAr = "اقرأ · اكتشف · حمّل",
                TaglineEn = "Read · Discover · Download",
                AboutAr = "مكتبة رقمية مفتوحة تتيح تصفح وتحميل كتب من الملك العام، مع تقييمات وتعليقات من القرّاء.",
                AboutEn = "An open digital library for browsing and downloading public-domain books, with reader ratings and comments.",
                Email = "hello@elibrary.local",
                Phone = "+60196493629",
                WhatsApp = "60196493629",
                AddressAr = "كوالالمبور، ماليزيا",
                AddressEn = "Kuala Lumpur, Malaysia"
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { NameAr = "رواية", NameEn = "Fiction", Slug = "fiction", SortOrder = 1 },
                new Category { NameAr = "كلاسيكيات", NameEn = "Classics", Slug = "classics", SortOrder = 2 },
                new Category { NameAr = "مغامرات", NameEn = "Adventure", Slug = "adventure", SortOrder = 3 },
                new Category { NameAr = "علوم وفلسفة", NameEn = "Science & Philosophy", Slug = "science-philosophy", SortOrder = 4 },
                new Category { NameAr = "شعر وأدب", NameEn = "Poetry & Literature", Slug = "poetry-literature", SortOrder = 5 }
            );
            await db.SaveChangesAsync();
        }

        // Refresh catalog when missing downloadable books
        var needsCatalog = !await db.Books.AnyAsync() ||
                           await db.Books.AllAsync(b => string.IsNullOrEmpty(b.DownloadUrl));
        if (!needsCatalog) return;

        db.BookComments.RemoveRange(db.BookComments);
        db.BookReactions.RemoveRange(db.BookReactions);
        db.BookRatings.RemoveRange(db.BookRatings);
        db.Books.RemoveRange(db.Books);
        await db.SaveChangesAsync();

        var cats = await db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
        Category Cat(string slug) => cats.First(c => c.Slug == slug);

        static string Cover(int gutenbergId) =>
            $"https://www.gutenberg.org/cache/epub/{gutenbergId}/pg{gutenbergId}.cover.medium.jpg";
        static string Epub(int gutenbergId) =>
            $"https://www.gutenberg.org/ebooks/{gutenbergId}.epub.images";

        var books = new List<Book>
        {
            new()
            {
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                Description = "A classic romance of manners following Elizabeth Bennet and Mr. Darcy in Regency England.",
                CoverUrl = Cover(1342),
                DownloadUrl = Epub(1342),
                Isbn = "9780141439518",
                PublishedYear = 1813,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("fiction").Id
            },
            new()
            {
                Title = "Frankenstein",
                Author = "Mary Shelley",
                Description = "The groundbreaking gothic novel about ambition, creation, and responsibility.",
                CoverUrl = Cover(84),
                DownloadUrl = Epub(84),
                Isbn = "9780486282114",
                PublishedYear = 1818,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("classics").Id
            },
            new()
            {
                Title = "Alice's Adventures in Wonderland",
                Author = "Lewis Carroll",
                Description = "Alice falls down a rabbit hole into a world of curious creatures and playful logic.",
                CoverUrl = Cover(11),
                DownloadUrl = Epub(11),
                Isbn = "9781503222687",
                PublishedYear = 1865,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("adventure").Id
            },
            new()
            {
                Title = "The Adventures of Sherlock Holmes",
                Author = "Arthur Conan Doyle",
                Description = "Twelve detective stories introducing Holmes and Watson's most famous cases.",
                CoverUrl = Cover(1661),
                DownloadUrl = Epub(1661),
                Isbn = "9781593080402",
                PublishedYear = 1892,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("fiction").Id
            },
            new()
            {
                Title = "Dracula",
                Author = "Bram Stoker",
                Description = "The definitive vampire novel told through letters, journals, and newspaper clippings.",
                CoverUrl = Cover(345),
                DownloadUrl = Epub(345),
                Isbn = "9780486411095",
                PublishedYear = 1897,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("classics").Id
            },
            new()
            {
                Title = "The Odyssey",
                Author = "Homer",
                Description = "The epic journey of Odysseus returning home after the Trojan War.",
                CoverUrl = Cover(1727),
                DownloadUrl = Epub(1727),
                Isbn = "9780140268867",
                PublishedYear = -800,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("classics").Id
            },
            new()
            {
                Title = "The Art of War",
                Author = "Sun Tzu",
                Description = "Ancient Chinese treatise on strategy, leadership, and conflict.",
                CoverUrl = Cover(132),
                DownloadUrl = Epub(132),
                Isbn = "9781599869773",
                PublishedYear = -500,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("science-philosophy").Id
            },
            new()
            {
                Title = "The Strange Case of Dr. Jekyll and Mr. Hyde",
                Author = "Robert Louis Stevenson",
                Description = "A chilling exploration of duality and the darker side of human nature.",
                CoverUrl = Cover(43),
                DownloadUrl = Epub(43),
                Isbn = "9780486266886",
                PublishedYear = 1886,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("fiction").Id
            },
            new()
            {
                Title = "Treasure Island",
                Author = "Robert Louis Stevenson",
                Description = "Pirates, buried treasure, and coming of age on the high seas.",
                CoverUrl = Cover(120),
                DownloadUrl = Epub(120),
                Isbn = "9780486275598",
                PublishedYear = 1883,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("adventure").Id
            },
            new()
            {
                Title = "Moby Dick",
                Author = "Herman Melville",
                Description = "Captain Ahab's obsessive hunt for the white whale.",
                CoverUrl = Cover(2701),
                DownloadUrl = Epub(2701),
                Isbn = "9781503280786",
                PublishedYear = 1851,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("classics").Id
            },
            new()
            {
                Title = "The Picture of Dorian Gray",
                Author = "Oscar Wilde",
                Description = "A beautiful young man sells his soul for eternal youth while his portrait bears the cost.",
                CoverUrl = Cover(174),
                DownloadUrl = Epub(174),
                Isbn = "9780486278070",
                PublishedYear = 1890,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("fiction").Id
            },
            new()
            {
                Title = "A Tale of Two Cities",
                Author = "Charles Dickens",
                Description = "Love and sacrifice set against the French Revolution in London and Paris.",
                CoverUrl = Cover(98),
                DownloadUrl = Epub(98),
                Isbn = "9780486406510",
                PublishedYear = 1859,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("classics").Id
            },
            new()
            {
                Title = "The Time Machine",
                Author = "H. G. Wells",
                Description = "A Victorian inventor travels to the distant future and finds a divided humanity.",
                CoverUrl = Cover(35),
                DownloadUrl = Epub(35),
                Isbn = "9780486284729",
                PublishedYear = 1895,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("adventure").Id
            },
            new()
            {
                Title = "Metamorphosis",
                Author = "Franz Kafka",
                Description = "Gregor Samsa wakes up transformed into an insect in this modernist masterpiece.",
                CoverUrl = Cover(5200),
                DownloadUrl = Epub(5200),
                Isbn = "9780553213690",
                PublishedYear = 1915,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("fiction").Id
            },
            new()
            {
                Title = "Leaves of Grass",
                Author = "Walt Whitman",
                Description = "A landmark poetry collection celebrating nature, democracy, and the self.",
                CoverUrl = Cover(1322),
                DownloadUrl = Epub(1322),
                Isbn = "9780486456768",
                PublishedYear = 1855,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("poetry-literature").Id
            },
            new()
            {
                Title = "Beyond Good and Evil",
                Author = "Friedrich Nietzsche",
                Description = "A provocative critique of traditional morality and truth.",
                CoverUrl = Cover(4363),
                DownloadUrl = Epub(4363),
                Isbn = "9780486298689",
                PublishedYear = 1886,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("science-philosophy").Id
            },
            new()
            {
                Title = "The Prince",
                Author = "Niccolò Machiavelli",
                Description = "A practical guide to power and statecraft from Renaissance Italy.",
                CoverUrl = Cover(1232),
                DownloadUrl = Epub(1232),
                Isbn = "9780486272740",
                PublishedYear = 1532,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("science-philosophy").Id
            },
            new()
            {
                Title = "The Jungle Book",
                Author = "Rudyard Kipling",
                Description = "Stories of Mowgli and the animals of the Indian jungle.",
                CoverUrl = Cover(236),
                DownloadUrl = Epub(236),
                Isbn = "9781503332546",
                PublishedYear = 1894,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("adventure").Id
            },
            new()
            {
                Title = "The Arabian Nights Entertainments",
                Author = "Anonymous / Andrew Lang",
                Description = "Classic tales from One Thousand and One Nights, including Aladdin and Sinbad.",
                CoverUrl = Cover(128),
                DownloadUrl = Epub(128),
                Isbn = "9780486218328",
                PublishedYear = 1898,
                Language = "en",
                IsFeatured = true,
                CategoryId = Cat("poetry-literature").Id
            },
            new()
            {
                Title = "Narrative of the Life of Frederick Douglass",
                Author = "Frederick Douglass",
                Description = "A powerful autobiography of escape from slavery and the fight for freedom.",
                CoverUrl = Cover(23),
                DownloadUrl = Epub(23),
                Isbn = "9780486284996",
                PublishedYear = 1845,
                Language = "en",
                IsFeatured = false,
                CategoryId = Cat("classics").Id
            }
        };

        // Fix invalid years for ancient works (validation expects 1000-2100 for admin forms;
        // store null for BCE-style years)
        foreach (var b in books.Where(b => b.PublishedYear is < 1000))
            b.PublishedYear = null;

        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }
}
