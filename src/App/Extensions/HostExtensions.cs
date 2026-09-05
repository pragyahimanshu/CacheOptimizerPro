using Microsoft.EntityFrameworkCore;

namespace App.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host)
        where TContext : DbContext
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                var dbContext = services.GetRequiredService<TContext>();

                logger.LogInformation("Applying migrations for DbContext {DbContext}", typeof(TContext).Name);

                dbContext.Database.Migrate();

                logger.LogInformation("Database migrations applied successfully for DbContext {DbContext}.", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on DbContext {DbContext}", typeof(TContext).Name);
            }
        }
        return host;
    }
}
