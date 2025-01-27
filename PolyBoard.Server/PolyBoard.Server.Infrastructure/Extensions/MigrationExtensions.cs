using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBoard.Server.Infrastructure.Data;

namespace PolyBoard.Infrastructure.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Failed to migrate: ", ex.ToString());
            }
            
        }
    }
}