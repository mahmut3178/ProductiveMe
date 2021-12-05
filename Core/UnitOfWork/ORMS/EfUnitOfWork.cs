using Core.Entities.Abstract;
using Core.UnitOfWork.Repositories;
using Core.UnitOfWork.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Core.UnitOfWork.ORMS
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        public EfUnitOfWork(DbContext context)
        {
            _context = context;
        }


        public IRepository<TEntity> GetEntityRepository<TEntity>() where TEntity : class, new()
        {
            return new EfRepository<TEntity>(_context);
        }

        public IQueryable<TEntity> GetQuery<TEntity>() where TEntity : Entity, new()
        {
            return _context.Set<TEntity>();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
