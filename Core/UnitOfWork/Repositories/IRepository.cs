using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.UnitOfWork.Repositories
{
    public interface IRepository<T> where T : class, new()
    {
        public T Get(Expression<Func<T, bool>> filter);
        public T GetById(int id);
        public T GetByIdGuid(Guid id);
        public IEnumerable<T> GetMany(Expression<Func<T, bool>> filter = null);
        public T Create(T entity);
        public T Update(T entity);
        public T Delete(T entity);

    }
}
