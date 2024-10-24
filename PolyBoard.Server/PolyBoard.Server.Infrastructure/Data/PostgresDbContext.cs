using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PolyBoard.Server.Core.Entities;

namespace PolyBoard.Server.Infrastructure.Data
{
    public class PostgresDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options) { }

        public override DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; init; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
