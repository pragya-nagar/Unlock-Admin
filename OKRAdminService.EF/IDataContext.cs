using System;

namespace OKRAdminService.EF
{
    public interface IDataContext : IDisposable
    {
        int SaveChanges();
        void SyncObjectState<T>( T entity ) where T : class, IObjectState;
        void SyncObjectStatePreCommit();
        void SyncObjectStatePostCommit();
    }
}
