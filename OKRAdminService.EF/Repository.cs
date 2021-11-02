using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OKRAdminService.EF
{
    public class Repository<T> : IRepositoryAsync<T> where T : class, IObjectState
    {
        protected readonly IDataContextAsync context;
        protected readonly DbSet<T> dbSet;

        public Repository(IDataContextAsync _context)
        {
            context = _context;
            dbSet = ((DbContext)context).Set<T>();
        }

        public virtual IDataContextAsync Context { get { return context; } }
        public DbSet<T> DbSet { get { return dbSet; } }
        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            return dbSet.SingleOrDefault(predicate);
        }
        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Where(predicate);
        }
        public void Add(T entity)
        {
            entity.ObjectStateEnum = ObjectState.Added;
            dbSet.Add(entity);
            context.SyncObjectState(entity);
        }
        public void Delete(T entity)
        {
            entity.ObjectStateEnum = ObjectState.Deleted;
            dbSet.Attach(entity);
            context.SyncObjectState(entity);
        }
        public IQueryable<T> GetQueryable()
        {
            return dbSet.AsQueryable();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public void Update(T entity)
        {
            entity.ObjectStateEnum = ObjectState.Modified;
            dbSet.Attach(entity);
            context.SyncObjectState(entity);
        }
        public async Task<T> FindOneAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.SingleOrDefaultAsync(predicate);
        }
        public async Task<IQueryable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(() => dbSet.Where(predicate));
        }
        public async Task<T> FindAsync(params object[] keyValues)
        {
            return await dbSet.FindAsync(keyValues);
        }
        public async Task<T> FindAsync(CancellationToken ct, params object[] keyValues)
        {
            return await dbSet.FindAsync( keyValues, ct);
        }
        public async Task<bool> DeleteAsync(params object[] keyValues)
        {
            return await DeleteAsync(CancellationToken.None, keyValues);
        }
        public async Task<bool> DeleteAsync(CancellationToken ct, params object[] keyValues)
        {
            var entity = await FindAsync(ct, keyValues);
            if (entity == null)
                return false;
            entity.ObjectStateEnum = ObjectState.Deleted;
            DbSet.Attach(entity);
            return true;
        }

        public int GetCount()
        {
            return dbSet.Count();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.CountAsync(predicate);
        }
    }

}
