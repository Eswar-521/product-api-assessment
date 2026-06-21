using Microsoft.EntityFrameworkCore;
using ProductApi.Infrastructure.Data;

namespace ProductApi.API.Extensions;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Database:AutoCreate"))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database schema is ready.");
                return;
            }
            catch (Exception exception) when (attempt < maxAttempts)
            {
                logger.LogWarning(exception, "Database is not ready. Retrying attempt {Attempt}/{MaxAttempts}.", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        await dbContext.Database.EnsureCreatedAsync();
    }
}
