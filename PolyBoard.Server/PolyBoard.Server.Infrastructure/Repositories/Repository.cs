using Microsoft.EntityFrameworkCore;
using PolyBoard.Server.Core.Interfaces;
using PolyBoard.Server.Core.Interfaces.Repositories;
using PolyBoard.Server.Infrastructure.Data;
using System.Linq.Expressions;

namespace PolyBoard.Server.Infrastructure.Repositories
{
    public class Repository<E>(PostgresDbContext dbContext) : IRepository<E> where E : class, IEntity, new()
    {
        //TODO implement generic methods
        public Task CreateAsync(E entity, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(E entity, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<E>> GetAllAsync(Expression<Func<E, E>>? selector = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<E?> GetByIdAsync(Guid id, Expression<Func<E, E>>? selector = null, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<E>()
                .Where(e => e.Id == id)
                .Select(GetSelector(selector))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Expression<Func<E, E>> GetSelector(Expression<Func<E, E>>? selector = null)
        {
            if (selector == null)
                selector = c => new E() { Id = c.Id };

            return selector;
        }

        public Task UpdateAsync(E entity, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}
