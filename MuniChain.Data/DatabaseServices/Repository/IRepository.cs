using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data.DatabaseServices.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(string id);
        Task Update(T entity);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression);
        Task Add(T entity);
        Task AddRange(IEnumerable<T> entities);
        Task Remove(string id);
        Task Remove(T entity);

        Task RemoveRange(IEnumerable<T> entities);
    }
}
