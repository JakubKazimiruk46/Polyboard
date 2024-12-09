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
        public DbSet<Game> Games { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<GameEvent> GameEvents { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<RentPrice> RentPrices { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<GameEvent>()
                .OwnsOne(
                    owner => owner.JsonBody, ownedNavigationBuilder =>
                    {
                        ownedNavigationBuilder.ToJson();
                    })
                .HasDiscriminator(ge => ge.Name);
            builder.Entity<Transaction>(e =>
            {
                // Relacja z Giver
                e.HasOne(t => t.Giver)
                    .WithMany(u => u.TransactionsAsGiver)
                    .HasForeignKey("GiverId")
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacja z Reciver
                e.HasOne(t => t.Reciver)
                    .WithMany(u => u.TransactionsAsReciver)
                    .HasForeignKey("ReciverId")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
