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
        public async Task CreateAsync(E entity, CancellationToken cancellation = default)
        {
            await dbContext.Set<E>().AddAsync(entity, cancellation);
            await dbContext.SaveChangesAsync(cancellation);
        }

        public async Task DeleteAsync(E entity, CancellationToken cancellation = default)
        {
            dbContext.Set<E>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellation);
        }
        public async Task<ICollection<E>> GetAsync(Expression<Func<E, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<E>()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<ICollection<E>> GetAllAsync(Expression<Func<E, E>>? selector = null, CancellationToken cancellationToken = default)
        {
            return await dbContext.Set<E>()
                .Select(GetSelector(selector))
                .ToListAsync(cancellationToken);
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

        public async Task UpdateAsync(E entity, CancellationToken cancellation = default)
        {
            dbContext.Set<E>().Update(entity);
            await dbContext.SaveChangesAsync(cancellation);
        }
    }
}
