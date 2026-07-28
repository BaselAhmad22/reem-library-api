using System.Text.RegularExpressions;
using System.Text;

namespace Elibrary.Api.Helpers;

public static class Validators
{
    public static string? Require(string? value, string field)
        => string.IsNullOrWhiteSpace(value) ? $"{field} is required." : null;

    public static string? Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Email is required.";
        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? null : "Invalid email.";
    }

    public static string? Url(string? value, bool optional = true)
    {
        if (string.IsNullOrWhiteSpace(value)) return optional ? null : "URL is required.";
        return Uri.TryCreate(value, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps)
            ? null : "Invalid URL.";
    }

    public static string? Phone(string? value, bool optional = true)
    {
        if (string.IsNullOrWhiteSpace(value)) return optional ? null : "Phone is required.";
        var digits = Regex.Replace(value, @"[^\d+]", "");
        return digits.Length >= 8 ? null : "Invalid phone number.";
    }

    public static string Slugify(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"-+", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(s))
            s = "category-" + Guid.NewGuid().ToString("N")[..6];
        return s;
    }

    public static string FirstError(params string?[] errors)
        => errors.FirstOrDefault(e => e != null) ?? string.Empty;
}
