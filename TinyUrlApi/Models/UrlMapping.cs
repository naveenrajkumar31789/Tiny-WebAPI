namespace TinyUrlApi.Models;

public class UrlMapping
{
    public int Id { get; set; }
    public string ShortCode { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int Hits { get; set; }
}
