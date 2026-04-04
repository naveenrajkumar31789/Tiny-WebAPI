using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Data;
using TinyUrlApi.Models;

namespace TinyUrlApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure SQLite database
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=tinyurls.db"));

        var app = builder.Build();

        // Ensure database is created
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapGet("/", () => Results.Redirect("/swagger"));

        // Generate a 6-character short code
        static string GenerateCode(int length = 6)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = Random.Shared;
            return new string(Enumerable.Range(0, length).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
        }

        // Create a new short URL (POST /api/add)
        app.MapPost("/api/add", async (Models.ShortenRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return Results.BadRequest(new { error = "Url is required." });

            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                return Results.BadRequest(new { error = "Invalid URL." });

            // generate unique 6-char code
            string code;
            var tries = 0;
            do
            {
                code = GenerateCode(6);
                tries++;
                if (tries > 50)
                    return Results.StatusCode(500);
            } while (await db.UrlMappings.AnyAsync(u => u.ShortCode == code));

            var mapping = new UrlMapping
            {
                ShortCode = code,
                OriginalUrl = uri.ToString(),
                CreatedAt = DateTime.UtcNow,
                Hits = 0
            };

            db.UrlMappings.Add(mapping);
            await db.SaveChangesAsync();

            var resp = new Models.ShortenResponse(mapping.ShortCode, mapping.OriginalUrl, mapping.CreatedAt);
            return Results.Created($"/api/public/{mapping.ShortCode}", resp);
        });

        // Delete a mapping by code (DELETE /api/delete/{code})
        app.MapDelete("/api/delete/{code}", async (string code, AppDbContext db) =>
        {
            var mapping = await db.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == code);
            if (mapping is null)
                return Results.NotFound();

            db.UrlMappings.Remove(mapping);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Delete all mappings (DELETE /api/delete-all)
        app.MapDelete("/api/delete-all", async (AppDbContext db) =>
        {
            db.UrlMappings.RemoveRange(db.UrlMappings);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Update mapping original URL (PUT /api/update/{code})
        app.MapPut("/api/update/{code}", async (string code, Models.ShortenRequest request, AppDbContext db) =>
        {
            var mapping = await db.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == code);
            if (mapping is null)
                return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Url))
                return Results.BadRequest(new { error = "Url is required." });

            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                return Results.BadRequest(new { error = "Invalid URL." });

            mapping.OriginalUrl = uri.ToString();
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Public listing of mappings (GET /api/public)
        app.MapGet("/api/public", async (AppDbContext db) =>
            await db.UrlMappings.OrderByDescending(u => u.CreatedAt).ToListAsync());

        // Redirect short code to original URL (GET /{code})
        app.MapGet("/{code}", async (string code, AppDbContext db) =>
        {
            var mapping = await db.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == code);
            if (mapping is null)
                return Results.NotFound();

            mapping.Hits++;
            await db.SaveChangesAsync();
            return Results.Redirect(mapping.OriginalUrl);
        });

        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}
