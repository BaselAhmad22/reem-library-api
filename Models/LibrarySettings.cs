namespace Elibrary.Api.Models;

public class LibrarySettings
{
    public int Id { get; set; }
    public string NameAr { get; set; } = "المكتبة الإلكترونية";
    public string NameEn { get; set; } = "Digital Library";
    public string TaglineAr { get; set; } = "اقرأ · اكتشف · تعلّم";
    public string TaglineEn { get; set; } = "Read · Discover · Learn";
    public string AboutAr { get; set; } = string.Empty;
    public string AboutEn { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressAr { get; set; } = string.Empty;
    public string AddressEn { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
}
