using System.Linq.Expressions;

namespace PolyBoard.Server.Core.Interfaces.Repositories
{
    public interface IRepository<E> where E : class, IEntity
    {
        Expression<Func<E, E>> GetSelector(Expression<Func<E, E>>? selector = null);
        
        Task<E?> GetByIdAsync(Guid id,
            Expression<Func<E, E>>? selector = null,
            CancellationToken cancellationToken = default);
        Task<ICollection<E>> GetAsync(Expression<Func<E, bool>> predicate, 
            CancellationToken cancellationToken = default);
        Task<ICollection<E>> GetAllAsync(Expression<Func<E, E>>? selector = null,
            CancellationToken cancellationToken = default);
        Task CreateAsync(E entity,
            CancellationToken cancellation = default);
        Task UpdateAsync(E entity,
            CancellationToken cancellation = default);
        Task DeleteAsync(E entity,
            CancellationToken cancellation = default);
    }
}
