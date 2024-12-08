using PolyBoard.Server.Core.Interfaces;

namespace PolyBoard.Server.Core.Entities
{
    public class UserAchivement : IEntity
    {
        public Guid Id { get; set; }
        public Achivement Achivement { get; set; }
        public decimal Progress { get; set; }
        public User User { get; set; }
    }
}
