using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
#if AZURE_FUNCTIONS
using Microsoft.Azure.Functions.Worker;
#endif
using TinyUrlApi.Data;

namespace TinyUrlApi.Functions
{
    public class CleanupFunction
    {
        private readonly ILogger _logger;

        public CleanupFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CleanupFunction>();
        }

        // Runs at minute 0 of every hour
#if AZURE_FUNCTIONS
        [Function("CleanupFunction")]
        public async Task Run([TimerTrigger("0 0 * * * *")] MyInfo myTimer)
#else
        // When AZURE_FUNCTIONS symbol is not defined this method can be called manually for testing
        public async Task Run(MyInfo? myTimer = null)
#endif
        {
            _logger.LogInformation($"CleanupFunction executed at: {DateTime.UtcNow}");

            try
            {
                // compute path to tinyurls.db located in the main web project folder
                var assemblyPath = Path.GetDirectoryName(typeof(CleanupFunction).Assembly.Location) ?? ".";
                var dbPath = Path.GetFullPath(Path.Combine(assemblyPath, "..", "..", "tinyurls.db"));

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                using var db = new AppDbContext(options);
                _logger.LogInformation($"Deleting all UrlMappings from DB: {dbPath}");

                db.UrlMappings.RemoveRange(db.UrlMappings);
                await db.SaveChangesAsync();

                _logger.LogInformation("All UrlMappings deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running cleanup function");
            }
        }

        public class MyInfo { }
    }
}
