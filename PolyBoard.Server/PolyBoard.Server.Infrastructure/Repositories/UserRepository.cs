using PolyBoard.Server.Core.Entities;
using PolyBoard.Server.Infrastructure.Data;

namespace PolyBoard.Server.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(PostgresDbContext dbContext) : base(dbContext)
        {
        }

        //custom methods implementations
    }
}
