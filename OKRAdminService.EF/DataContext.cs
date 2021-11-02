using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace OKRAdminService.EF
{
    public class DataContext : DbContext, IDataContextAsync
    {
        public DataContext(DbContextOptions<OkrAdminDbContext> options) : base(options)
        {
        }

        public DataContext()
        {
        }
        public override int SaveChanges()
        {
            SyncObjectStatePreCommit();
            var changes = base.SaveChanges();
            SyncObjectStatePostCommit();
            return changes;
        }
        public Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SyncObjectStatePreCommit();
            var changes = await base.SaveChangesAsync(cancellationToken);
            SyncObjectStatePostCommit();
            return changes;
        }
        public void SyncObjectState<T>(T entity) where T : class, IObjectState
        {
            Entry(entity).State = StateHelper.ConvertState(entity.ObjectStateEnum);
        }
        public void SyncObjectStatePreCommit()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                entry.State = StateHelper.ConvertState(((IObjectState)entry.Entity).ObjectStateEnum);
            }
        }
        public void SyncObjectStatePostCommit()
        {
            foreach (var entry in ChangeTracker.Entries())
                ((IObjectState)entry.Entity).ObjectStateEnum = StateHelper.ConvertState(entry.State);
        }
    }

}
