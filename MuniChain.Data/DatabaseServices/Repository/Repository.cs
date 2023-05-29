using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.DatabaseServices.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IDbContextFactory<SqlDbContext> _factory;
        public Repository(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(T entity)
        {
            using var _dbContext = _factory.CreateDbContext();

            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            using var _dbContext = _factory.CreateDbContext();

            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddRange(IEnumerable<T> entities)
        {
            using var _dbContext = _factory.CreateDbContext();

            _dbContext.Set<T>().AddRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression)
        {
            using var _dbContext = _factory.CreateDbContext();
            return await _dbContext.Set<T>().Where(expression).ToListAsync();
        }

        public async Task<T> GetById(string id)
        {
            using var _dbContext = _factory.CreateDbContext();

            return await _dbContext.Set<T>().FindAsync(id);

        }

        public async Task Remove(string id)
        {
            using var _dbContext = _factory.CreateDbContext();

            var existing = await GetById(id);

            if (existing != null)
            {
                _dbContext.Set<T>().Remove(existing);
                await _dbContext.SaveChangesAsync();
            }

            return;
        }

        public async Task Remove(T entity)
        {
            using var _dbContext = _factory.CreateDbContext();

            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();

            return;
        }

        public async Task RemoveRange(IEnumerable<T> entities)
        {
            using var _dbContext = _factory.CreateDbContext();

            _dbContext.Set<T>().RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }
    }
}
