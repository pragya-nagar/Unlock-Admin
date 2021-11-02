using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace OKRAdminService.EF
{
    public interface IRepository<T> where T : class
    {
        IDataContextAsync Context { get; }
        DbSet<T> DbSet { get; }
        void Add( T entity );
        void Delete( T entity );
        T FindOne( Expression<Func<T, bool>> predicate );
        IQueryable<T> FindBy( Expression<Func<T, bool>> predicate );
        IQueryable<T> GetQueryable();
        void Update( T entity );
        int GetCount();
    }
}
