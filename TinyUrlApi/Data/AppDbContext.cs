using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Models;

namespace TinyUrlApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UrlMapping> UrlMappings { get; set; } = null!;
}
