using System;

namespace OKRAdminService.EF
{
    public interface IUnitOfWork : IDisposable
    {
        IDataContextAsync DataContext { get; }
        IOperationStatus SaveChanges();
        void DisposeIt(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, IObjectState;
        bool Commit();
        void Rollback();
    }
}
