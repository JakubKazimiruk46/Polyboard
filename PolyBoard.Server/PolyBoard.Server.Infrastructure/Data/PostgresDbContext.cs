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
            builder.Entity<UserAchievement>()
                .HasIndex(ua => new { ua.UserId, ua.AchievementId})
                .IsUnique();
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
            builder.Entity<Achievement>().HasData(new List<Achievement>
            {
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Dziekan wydziału inwestycji",
                    Requirement = "Zbuduj hotele na wszystkich polach jednego koloru",
                    NumberToReach = 3,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Pierwszy krok",
                    Requirement = "Wygraj grę",
                    NumberToReach = 1,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Prześladowany przez los",
                    Requirement = "Traf do dziekanatu trzy razy w jednej grze",
                    NumberToReach = 3,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Magnat inwestycyjny",
                    Requirement = "Kup wszystkie pola z trzech kolorów w jednej grze",
                    NumberToReach = 9,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Dubletowy szczęściarz",
                    Requirement = "Wyrzuć dublet trzy razy z rzędu co najmniej raz",
                    NumberToReach = 3,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Budowniczy imperium",
                    Requirement = "Zbuduj co najmniej 20 budynków w jednej grze",
                    NumberToReach = 20,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Podatnik Roku",
                    Requirement = "Zapłać co najmniej 2000 punktów ECTS w podatkach w jednej grze",
                    NumberToReach = 2000,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Akademicki inwestor",
                    Requirement = "Kup wszystkie akademiki i miejsca społeczne w jednej grze",
                    NumberToReach = 6,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Finansowy mistrz",
                    Requirement = "Wygraj grę, unikając płacenia kar i podatków",
                    NumberToReach = 1,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Bezlitosny",
                    Requirement = "Zrujnuj finansowo przeciwnika w pierwszych 5 turach",
                    NumberToReach = 1,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Ekspert ds. dywersyfikacji",
                    Requirement = "Zdobądź co najmniej jedną nieruchomość każdego koloru",
                    NumberToReach = 8,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Strategiczny profesor",
                    Requirement = "Zdobądź nieruchomości o łącznej wartości co najmniej 3000 punktów ECTS w jednej grze",
                    NumberToReach = 3000,
                },
                new Achievement
                {
                    Id = Guid.NewGuid(),
                    Name = "Milioner",
                    Requirement = "Zgromadź łącznie 100.000 punktów ECTS",
                    NumberToReach = 100000,
                }
            });

        }
    }
}
