namespace TinyUrlApi.Models;

public record ShortenResponse(string ShortCode, string OriginalUrl, System.DateTime CreatedAt);
