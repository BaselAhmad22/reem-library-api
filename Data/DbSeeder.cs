using Elibrary.Api.Models;
using Elibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, BookPdfService pdfs)
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
                AboutAr = "مكتبة رقمية مفتوحة تتيح تصفح وتحميل كتب من الملك العام بصيغة PDF عربية وإنجليزية.",
                AboutEn = "An open digital library for browsing and downloading public-domain books as Arabic and English PDFs.",
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

        var needsCatalog = !await db.Books.AnyAsync()
            || await db.Books.AnyAsync(b =>
                string.IsNullOrEmpty(b.DownloadUrlAr)
                || b.DownloadUrl.Contains("epub", StringComparison.OrdinalIgnoreCase)
                || !b.DownloadUrl.Contains("/books/", StringComparison.OrdinalIgnoreCase));

        if (!needsCatalog)
        {
            // Still ensure PDF files exist on disk for current catalog
            foreach (var book in await db.Books.Where(b => b.GutenbergId != null).ToListAsync())
            {
                var gid = book.GutenbergId!.Value;
                await pdfs.EnsurePdfsAsync(
                    gid,
                    book.Title,
                    string.IsNullOrWhiteSpace(book.TitleAr) ? book.Title : book.TitleAr,
                    book.Author,
                    ArabicReadingTexts.ForBook(gid, string.IsNullOrWhiteSpace(book.TitleAr) ? book.Title : book.TitleAr, book.Author));
            }
            return;
        }

        db.BookComments.RemoveRange(db.BookComments);
        db.BookReactions.RemoveRange(db.BookReactions);
        db.BookRatings.RemoveRange(db.BookRatings);
        db.Books.RemoveRange(db.Books);
        await db.SaveChangesAsync();

        var cats = await db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
        Category Cat(string slug) => cats.First(c => c.Slug == slug);
        static string Cover(int gutenbergId) =>
            $"https://www.gutenberg.org/cache/epub/{gutenbergId}/pg{gutenbergId}.cover.medium.jpg";

        var seed = new (int Id, string Title, string TitleAr, string Author, string Desc, string Cat, bool Featured, int? Year, string Isbn)[]
        {
            (1342, "Pride and Prejudice", "كبرياء وتحامل", "Jane Austen", "A classic romance of manners following Elizabeth Bennet and Mr. Darcy.", "fiction", true, 1813, "9780141439518"),
            (84, "Frankenstein", "فرانكشتاين", "Mary Shelley", "The groundbreaking gothic novel about ambition, creation, and responsibility.", "classics", true, 1818, "9780486282114"),
            (11, "Alice's Adventures in Wonderland", "أليس في بلاد العجائب", "Lewis Carroll", "Alice falls down a rabbit hole into a world of curious creatures.", "adventure", true, 1865, "9781503222687"),
            (1661, "The Adventures of Sherlock Holmes", "مغامرات شيرلوك هولمز", "Arthur Conan Doyle", "Twelve detective stories introducing Holmes and Watson.", "fiction", true, 1892, "9781593080402"),
            (345, "Dracula", "دراكيولا", "Bram Stoker", "The definitive vampire novel told through letters and journals.", "classics", false, 1897, "9780486411095"),
            (1727, "The Odyssey", "الأوديسة", "Homer", "The epic journey of Odysseus returning home after the Trojan War.", "classics", true, null, "9780140268867"),
            (132, "The Art of War", "فن الحرب", "Sun Tzu", "Ancient Chinese treatise on strategy, leadership, and conflict.", "science-philosophy", false, null, "9781599869773"),
            (43, "Dr. Jekyll and Mr. Hyde", "جيكل وهايد", "Robert Louis Stevenson", "A chilling exploration of duality and the darker side of human nature.", "fiction", false, 1886, "9780486266886"),
            (120, "Treasure Island", "جزيرة الكنز", "Robert Louis Stevenson", "Pirates, buried treasure, and coming of age on the high seas.", "adventure", true, 1883, "9780486275598"),
            (2701, "Moby Dick", "موبي ديك", "Herman Melville", "Captain Ahab's obsessive hunt for the white whale.", "classics", false, 1851, "9781503280786"),
            (174, "The Picture of Dorian Gray", "صورة دوريان غراي", "Oscar Wilde", "A beautiful young man sells his soul for eternal youth.", "fiction", true, 1890, "9780486278070"),
            (98, "A Tale of Two Cities", "قصة مدينتين", "Charles Dickens", "Love and sacrifice set against the French Revolution.", "classics", false, 1859, "9780486406510"),
            (35, "The Time Machine", "آلة الزمن", "H. G. Wells", "A Victorian inventor travels to the distant future.", "adventure", false, 1895, "9780486284729"),
            (5200, "Metamorphosis", "المسخ", "Franz Kafka", "Gregor Samsa wakes up transformed into an insect.", "fiction", true, 1915, "9780553213690"),
            (1322, "Leaves of Grass", "أوراق العشب", "Walt Whitman", "A landmark poetry collection celebrating nature and the self.", "poetry-literature", false, 1855, "9780486456768"),
            (4363, "Beyond Good and Evil", "ما وراء الخير والشر", "Friedrich Nietzsche", "A provocative critique of traditional morality and truth.", "science-philosophy", false, 1886, "9780486298689"),
            (1232, "The Prince", "الأمير", "Niccolò Machiavelli", "A practical guide to power and statecraft from Renaissance Italy.", "science-philosophy", false, 1532, "9780486272740"),
            (236, "The Jungle Book", "كتاب الأدغال", "Rudyard Kipling", "Stories of Mowgli and the animals of the Indian jungle.", "adventure", true, 1894, "9781503332546"),
            (128, "The Arabian Nights", "ألف ليلة وليلة", "Anonymous / Andrew Lang", "Classic tales from One Thousand and One Nights.", "poetry-literature", true, 1898, "9780486218328"),
            (23, "Narrative of Frederick Douglass", "سيرة فريدريك دوغلاس", "Frederick Douglass", "A powerful autobiography of escape from slavery.", "classics", false, 1845, "9780486284996"),
        };

        var books = new List<Book>();
        foreach (var s in seed)
        {
            await pdfs.EnsurePdfsAsync(
                s.Id,
                s.Title,
                s.TitleAr,
                s.Author,
                ArabicReadingTexts.ForBook(s.Id, s.TitleAr, s.Author));

            books.Add(new Book
            {
                Title = s.Title,
                TitleAr = s.TitleAr,
                Author = s.Author,
                Description = s.Desc,
                CoverUrl = Cover(s.Id),
                DownloadUrl = pdfs.EnRelativePath(s.Id),
                DownloadUrlAr = pdfs.ArRelativePath(s.Id),
                GutenbergId = s.Id,
                Isbn = s.Isbn,
                PublishedYear = s.Year is < 1000 ? null : s.Year,
                Language = "en",
                IsFeatured = s.Featured,
                CategoryId = Cat(s.Cat).Id
            });
        }

        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }
}
