using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.UnitOfWork.Repositories.EntityFramework
{
    public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _set;
        public EfRepository(DbContext context)
        {
            _context = context;
            _set = _context.Set <TEntity>();
        }
        public TEntity Create(TEntity entity)
        {
            _set.Add(entity);
            return entity;
        }
        public TEntity Update(TEntity entity)
        {
            _set.Update(entity);
            return entity;
        }
        public TEntity Delete(TEntity entity)
        {
            _set.Remove(entity);
            return entity;
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filter)
        {
            return _set.FirstOrDefault(filter);
        }

        public TEntity GetById(int id)
        {
            return _set.Find(id);
        }

        public TEntity GetByIdGuid(Guid id)
        {
           return _set.Find(id);  
        }

        public IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null ? _set.ToList() : _set.Where(filter).ToList();
        }

    }
}
