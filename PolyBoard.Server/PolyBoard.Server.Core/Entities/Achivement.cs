using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class Achivement : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public decimal? NumberToReach { get; set; }

        public List<UserAchivement> UserAchivements { get; set; }
    }
}
