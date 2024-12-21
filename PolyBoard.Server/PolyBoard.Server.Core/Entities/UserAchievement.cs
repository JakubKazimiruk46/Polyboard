using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class UserAchievement : IEntity
    {
        public Guid Id { get; set; }
        public decimal Progress { get; set; }
        public Guid AchievementId { get; set; }
        public Guid UserId { get; set; }
    }
}
