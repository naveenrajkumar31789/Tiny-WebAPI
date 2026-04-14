namespace TinyUrlApi.Models;

public record ShortenRequest(string Url, bool IsPrivate = false);
