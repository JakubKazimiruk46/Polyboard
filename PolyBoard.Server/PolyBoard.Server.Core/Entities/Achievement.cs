using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class Achievement : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public decimal? NumberToReach { get; set; }

        public ICollection<UserAchievement> UserAchievements { get; set; } = [];
    }
}
