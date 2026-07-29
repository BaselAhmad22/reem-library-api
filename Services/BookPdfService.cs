using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Elibrary.Api.Services;

public class BookPdfService
{
    private static readonly object FontLock = new();
    private static bool _fontsRegistered;

    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BookPdfService> _logger;

    static BookPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public BookPdfService(IWebHostEnvironment env, IHttpClientFactory httpClientFactory, ILogger<BookPdfService> logger)
    {
        _env = env;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string BooksRoot => Path.Combine(_env.ContentRootPath, "wwwroot", "books");
    public string FontsRoot => Path.Combine(_env.ContentRootPath, "wwwroot", "fonts");

    public string EnRelativePath(int gutenbergId) => $"/books/en/{gutenbergId}.pdf";
    public string ArRelativePath(int gutenbergId) => $"/books/ar/{gutenbergId}.pdf";

    public async Task EnsurePdfsAsync(
        int gutenbergId,
        string titleEn,
        string titleAr,
        string author,
        string arabicBody,
        CancellationToken ct = default)
    {
        EnsureFontsRegistered();
        Directory.CreateDirectory(Path.Combine(BooksRoot, "en"));
        Directory.CreateDirectory(Path.Combine(BooksRoot, "ar"));

        var enPath = Path.Combine(BooksRoot, "en", $"{gutenbergId}.pdf");
        var arPath = Path.Combine(BooksRoot, "ar", $"{gutenbergId}.pdf");

        if (!File.Exists(enPath) || new FileInfo(enPath).Length < 1000)
        {
            var enText = await FetchGutenbergTextAsync(gutenbergId, ct);
            if (string.IsNullOrWhiteSpace(enText))
                enText = $"{titleEn}\n\nby {author}\n\n(Public-domain source temporarily unavailable. Please try again later.)";
            GeneratePdf(enPath, titleEn, author, enText, arabic: false);
        }

        if (!File.Exists(arPath) || new FileInfo(arPath).Length < 1000)
        {
            var arText = string.IsNullOrWhiteSpace(arabicBody)
                ? $"كتاب: {titleAr}\nالمؤلف: {author}\n\nنص عربي من الملك العام للقراءة والتحميل."
                : arabicBody;
            GeneratePdf(arPath, titleAr, author, arText, arabic: true);
        }
    }

    private void EnsureFontsRegistered()
    {
        if (_fontsRegistered) return;
        lock (FontLock)
        {
            if (_fontsRegistered) return;
            foreach (var file in new[]
                     {
                         "NotoSans-Regular.ttf", "NotoSans-Bold.ttf",
                         "NotoNaskhArabic-Regular.ttf", "NotoNaskhArabic-Bold.ttf"
                     })
            {
                var path = Path.Combine(FontsRoot, file);
                if (!File.Exists(path))
                    throw new InvalidOperationException($"Missing font: {path}");
                FontManager.RegisterFont(File.OpenRead(path));
            }
            _fontsRegistered = true;
        }
    }

    private async Task<string> FetchGutenbergTextAsync(int gutenbergId, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("gutenberg");
            var url = $"https://www.gutenberg.org/cache/epub/{gutenbergId}/pg{gutenbergId}.txt";
            using var res = await client.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode) return "";
            var raw = await res.Content.ReadAsStringAsync(ct);
            return CleanGutenbergText(raw);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed fetching Gutenberg text for {Id}", gutenbergId);
            return "";
        }
    }

    private static string CleanGutenbergText(string raw)
    {
        var start = raw.IndexOf("*** START OF", StringComparison.OrdinalIgnoreCase);
        var end = raw.IndexOf("*** END OF", StringComparison.OrdinalIgnoreCase);
        if (start >= 0)
        {
            var nl = raw.IndexOf('\n', start);
            raw = nl > 0 ? raw[(nl + 1)..] : raw;
        }
        if (end > 0)
            raw = raw[..end];

        raw = raw.Replace("\r\n", "\n").Trim();
        const int max = 90000;
        if (raw.Length > max)
            raw = raw[..max] + "\n\n[… truncated for portable PDF size …]";
        return raw;
    }

    private static void GeneratePdf(string path, string title, string author, string body, bool arabic)
    {
        var fontName = arabic ? "Noto Naskh Arabic" : "Noto Sans";
        var paragraphs = body
            .Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l => l.Length > 0)
            .Take(1200)
            .ToList();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(fontName).FontSize(arabic ? 12 : 11).LineHeight(1.45f));
                if (arabic) page.ContentFromRightToLeft();

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontFamily(fontName).Bold().FontSize(arabic ? 18 : 16);
                    col.Item().Text(author).FontFamily(fontName).FontSize(11).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(16).Column(col =>
                {
                    foreach (var p in paragraphs)
                        col.Item().PaddingBottom(6).Text(p).FontFamily(fontName);
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.DefaultTextStyle(x => x.FontFamily(fontName).FontSize(9).FontColor(Colors.Grey.Darken1));
                    txt.Span(arabic ? "مكتبة ريم — ملك عام  ·  صفحة " : "Reem Library — Public Domain  ·  Page ");
                    txt.CurrentPageNumber();
                });
            });
        }).GeneratePdf(path);
    }
}
