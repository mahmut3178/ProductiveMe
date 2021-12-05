using Core.Entities.Abstract;
using Core.UnitOfWork.Repositories;
namespace Core.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IQueryable<TEntity> GetQuery<TEntity>() where TEntity : Entity, new();
        IRepository<TEntity> GetEntityRepository<TEntity>() where TEntity : class, new();
        void Save();
        Task SaveAsync();
    }
}
